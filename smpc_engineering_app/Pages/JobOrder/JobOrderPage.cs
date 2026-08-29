using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Services.Setup;
using smpc_engineering_app.Services.Helpers;
using smpc_engineering_app.Services;
using System.Diagnostics;
using System.IO;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;

namespace smpc_engineering_app.Pages.JobOrder

{
    public partial class JobOrderPage : UserControl
    {
        GeneralService<JobOrderModel> generalGetJobOrder;
        GeneralService<JobOrderModel> generalSaveJobOrder;
        GeneralService<EngineerUserModel> generalEngrUsers;
        private List<JobOrderModel> _pldata;
        private List<EngineerUserModel> _engrdata;
        private DataTable _plTable;
        private bool _isEditing;

        // Bugs #198/#199 (Trello): shared by all three tabs' grids - a cell
        // formatting/type-conversion error (e.g. a blank Assigned Engr. cell)
        // used to fall through to WinForms' own default error dialog, which
        // can stack multiple copies and cascade into an unhandled exception
        // once dismissed. Suppress the dialog and swallow the error instead -
        // the cell just keeps showing whatever it already had.
        private void dgv_pl_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            e.Cancel = true;
        }
        private bool _pendingLoaded = false;
        private bool _ongoingLoaded = false;
        private bool _finishedLoaded = false;
        private HashSet<int> _editedRowIndices = new HashSet<int>();
        private string placeHolderText = "Job Order Search...";
        private DateCellPickerOverlay _duePickerOverlay;

        public JobOrderPage()
        {
            InitializeComponent();

            dgv_pl_pending.DataError += (s, e) => e.ThrowException = false;

            // In constructor, replace CellValueChanged with CellEndEdit
            dgv_pl_pending.CellEndEdit += Dgv_CellEndEdit;
            dgv_pl_ongoing.CellEndEdit += Dgv_CellEndEdit;
            dgv_pl_finished.CellEndEdit += Dgv_CellEndEdit;

            // DataGridView has no built-in date-picker column - floats a real
            // DateTimePicker over due_pending's active cell instead of relying on
            // free-typed text (see DateCellPickerOverlay's own comment for why that
            // was also silently contributing to the "Due date is required" confusion).
            _duePickerOverlay = new DateCellPickerOverlay(dgv_pl_pending, "due_pending");

            Helpers.DataGridViewDocumentFormatter.DataGridViewDocumentFormat(dgv_pl_pending, "sales_order_pending", "SO");
            Helpers.DataGridViewDocumentFormatter.DataGridViewDocumentFormat(dgv_pl_ongoing, "sales_order_ongoing", "SO");
            Helpers.DataGridViewDocumentFormatter.DataGridViewDocumentFormat(dgv_pl_finished, "sales_order_finished", "SO");
        }

        private void SetEditableColumns(DataGridView dgv, bool isEdit, params string[] editableColumns)
        {
            foreach (var colName in editableColumns)
            {
                if (dgv.Columns.Contains(colName))
                {
                    var column = dgv.Columns[colName];
                    column.ReadOnly = !isEdit;
                    column.DefaultCellStyle.BackColor = column.ReadOnly ? Color.Gainsboro : Color.White;
                }
            }
        }

        private void SetEditMode(bool enable, bool isNewMode = false)
        {
            _isEditing = enable;

            if (enable)
            {
                _editedRowIndices.Clear();

                // Each grid is normally bound to a live DataView whose RowFilter reads the
                // very columns this mode lets the user edit (Pending: a_engr/due; Ongoing:
                // status - see BindPendingData/BindOngoingData). Editing one of those fields
                // re-evaluates the filter immediately, so the row jumps position or vanishes
                // out from under the user mid-edit, before Save is ever clicked - and since
                // _editedRowIndices tracks plain row *positions*, a reshuffle silently makes
                // it point at the wrong row too (this is what produced "Row 1: Due date is
                // required" against a row the user never touched). Freezing each grid onto a
                // detached snapshot for the duration of the edit keeps every row exactly
                // where the user left it; Save and Cancel both end by reloading fresh data
                // (LoadJobOrder), which naturally re-buckets rows into the right tab under a
                // live view again.
                FreezeForEdit(dgv_pl_pending);
                FreezeForEdit(dgv_pl_ongoing);
                FreezeForEdit(dgv_pl_finished);
            }

            SetEditableColumns(dgv_pl_pending, enable, "due_pending", "cmb_a_engr_pending");
            SetEditableColumns(dgv_pl_ongoing, enable, "status_ongoing");
            SetEditableColumns(dgv_pl_finished, enable, "serial_no_finished");

            _duePickerOverlay.SetEditingMode(enable);

            // buttons
            string[] editButtons = { "btn_save", "btn_cancel" };
            string[] navButtons = { "btn_edit" };

            Helpers.SetButtonVisibility(
                toolStrip1,
                visibleButtons: enable ? editButtons : navButtons,
                hiddenButtons: enable ? navButtons : editButtons
            );
        }

        private void FreezeForEdit(DataGridView dgv)
        {
            if (dgv.DataSource is DataView view)
            {
                dgv.DataSource = view.ToTable();
            }
        }

        private void btn_edit_Click(object sender, EventArgs e)
        {
            SetEditMode(true);
        }

        private async void btn_cancel_Click(object sender, EventArgs e)
        {
            SetEditMode(false);

            await LoadJobOrder();
        }

        private async void btn_save_Click(object sender, EventArgs e)
        {
            btn_save.Enabled = false;
            btn_cancel.Enabled = false;

            try
            {
                dgv_pl_pending.EndEdit();
                dgv_pl_ongoing.EndEdit();
                dgv_pl_finished.EndEdit();

                if (tab_container.SelectedIndex == 0)
                {
                    if (!ValidateDueDates(dgv_pl_pending, "due_pending"))
                    {
                        btn_save.Enabled = true;
                        btn_cancel.Enabled = true;
                        return;
                    }
                }

                DataGridView currentDgv;
                string postfix;

                if (tab_container.SelectedIndex == 0)
                {
                    currentDgv = dgv_pl_pending;
                    postfix = "_pending";
                }
                else if (tab_container.SelectedIndex == 1)
                {
                    currentDgv = dgv_pl_ongoing;
                    postfix = "_ongoing";
                }
                else if (tab_container.SelectedIndex == 2)
                {
                    currentDgv = dgv_pl_finished;
                    postfix = "_finished";
                }
                else
                {
                    currentDgv = dgv_pl_pending;
                    postfix = "_pending";
                }

                // Strip postfix from column names before mapping
                var strippedColumns = new Dictionary<DataGridViewColumn, string>();
                foreach (DataGridViewColumn col in currentDgv.Columns)
                {
                    if (col.Name.EndsWith(postfix, StringComparison.OrdinalIgnoreCase))
                    {
                        string stripped = col.Name.Substring(0, col.Name.Length - postfix.Length);
                        strippedColumns[col] = col.Name;
                        col.Name = stripped;
                    }
                }

                void RestoreColumnNames()
                {
                    foreach (var kvp in strippedColumns)
                    {
                        kvp.Key.Name = kvp.Value;
                    }
                }

                var jobOrder = Helpers.DatagridviewMapper.BuildModelsFromData<JobOrderModel>(currentDgv, _editedRowIndices);

                if (tab_container.SelectedIndex == 0)
                {
                    int listIndex = 0;
                    foreach (int rowIndex in _editedRowIndices)
                    {
                        var row = currentDgv.Rows[rowIndex];
                        if (row.IsNewRow) continue;

                        var engrIdCell = row.Cells["cmb_a_engr"];
                        if (engrIdCell?.Value != null && int.TryParse(engrIdCell.Value.ToString(), out int engrId))
                        {
                            jobOrder[listIndex].engr_id = engrId;
                        }

                        listIndex++;
                    }
                }

                Helpers.Loading.ShowLoading(dgv_pl_pending, "Saving data...");
                Helpers.Loading.ShowLoading(dgv_pl_ongoing, "Saving data...");
                Helpers.Loading.ShowLoading(dgv_pl_finished, "Saving data...");

                // Check if any row has an existing ID → Update, otherwise → Insert
                bool isUpdate = jobOrder.Any(x => x.id != 0);

                ApiResponseModel result;

                generalSaveJobOrder = new GeneralService<JobOrderModel>(ApiEndPoints.JOB_ORDER);

                if (isUpdate)
                {
                    result = await generalSaveJobOrder.UpdateList(jobOrder);
                }
                else
                {
                    result = await generalSaveJobOrder.InsertList(jobOrder);
                }

                RestoreColumnNames();

                if (!result.success)
                {
                    Helpers.ShowDialogMessage("error", isUpdate ? "Job Order not updated." : "Job Order not created.");
                    return;
                }

                Helpers.ShowDialogMessage("success", isUpdate ? "Job Order updated successfully." : "Job Order created successfully.");

                _editedRowIndices.Clear();
                SetEditMode(false);
                _pendingLoaded = false;
                _ongoingLoaded = false;
                _finishedLoaded = false;
                await LoadJobOrder();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to save: {ex.Message}");
            }
            finally
            {
                btn_save.Enabled = true;
                btn_cancel.Enabled = true;

                Helpers.Loading.HideLoading(dgv_pl_pending);
                Helpers.Loading.HideLoading(dgv_pl_ongoing);
                Helpers.Loading.HideLoading(dgv_pl_finished);
            }
        }

        private bool ValidateDueDates(DataGridView dgv, string dueColumnName)
        {
            foreach (int rowIndex in _editedRowIndices)
            {
                var row = dgv.Rows[rowIndex];
                if (row.IsNewRow) continue;

                var dueCell = row.Cells[dueColumnName];
                string rawValue = dueCell?.Value?.ToString()?.Trim() ?? string.Empty;

                // If the due cell was edited but is now empty, it means it failed the format check
                if (string.IsNullOrWhiteSpace(rawValue))
                {
                    dgv.CurrentCell = dueCell;
                    Helpers.ShowDialogMessage("error", $"Row {rowIndex + 1}: Due date is required and must be in MM/dd/yyyy format.");
                    return false;
                }

                if (!DateTime.TryParseExact(rawValue, "MM/dd/yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out _))
                {
                    dgv.CurrentCell = dueCell;
                    Helpers.ShowDialogMessage("error", $"Row {rowIndex + 1}: Invalid due date \"{rawValue}\". Please use MM/dd/yyyy format.");
                    return false;
                }
            }
            return true;
        }

        private async void JobOrderPage_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_pl_pending, "Fetching data...");
                Helpers.Loading.ShowLoading(dgv_pl_ongoing, "Fetching data...");
                Helpers.Loading.ShowLoading(dgv_pl_finished, "Fetching data...");
                await LoadJobOrder();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_pl_pending);
                Helpers.Loading.HideLoading(dgv_pl_ongoing);
                Helpers.Loading.HideLoading(dgv_pl_finished);
            }
        }

        private async Task LoadJobOrder()
        {
            try
            {
                generalGetJobOrder = new GeneralService<JobOrderModel>(ApiEndPoints.JOB_ORDER + "/0");
                _pldata = await generalGetJobOrder.GetAsList();

                generalEngrUsers = new GeneralService<EngineerUserModel>(ApiEndPoints.ENGINEER_LIST);
                _engrdata = await generalEngrUsers.GetAsList();

                if (_pldata != null)
                {
                    _plTable = Helpers.ToDataTable(_pldata);
                    ApplyIncompleteToMaterials();

                    // Reset all loaded flags so tabs rebind with fresh data
                    _pendingLoaded = false;
                    _ongoingLoaded = false;
                    _finishedLoaded = false;

                    // Rebind only the currently visible tab
                    switch (tab_container.SelectedIndex)
                    {
                        case 0: BindPendingData(); break;
                        case 1: BindOngoingData(); break;
                        case 2: BindFinishedData(); break;
                    }
                }
                else
                {
                    dgv_pl_pending.DataSource = null;
                    dgv_pl_ongoing.DataSource = null;
                    dgv_pl_finished.DataSource = null;
                }
            }
            catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "No Job Order found.");
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
        }

        private void BindPendingData()
        {
            if (_pldata == null) return;

            var aEngrColumn = (DataGridViewComboBoxColumn)dgv_pl_pending.Columns["cmb_a_engr_pending"];
            aEngrColumn.DataSource = _engrdata;
            aEngrColumn.ValueMember = "id";
            aEngrColumn.DisplayMember = "full_name";
            aEngrColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
            aEngrColumn.FlatStyle = FlatStyle.Flat;
            aEngrColumn.DefaultCellStyle.NullValue = "";
            aEngrColumn.DataPropertyName = "engr_id";

            dgv_pl_pending.AutoGenerateColumns = false;

            var view = new DataView(_plTable);
            view.RowFilter = "(a_engr IS NULL OR a_engr = '') OR (due IS NULL OR due = '')";

            dgv_pl_pending.DataSource = view;
            _pendingLoaded = true;
        }

        private void BindOngoingData()
        {
            if (_pldata == null) return;

            dgv_pl_ongoing.AutoGenerateColumns = false;

            var view = new DataView(_plTable);
            view.RowFilter = "a_engr IS NOT NULL AND a_engr <> '' AND due IS NOT NULL AND due <> '' AND status <> 'COMPLETE'";

            dgv_pl_ongoing.DataSource = view;
            _ongoingLoaded = true;
        }

        private void BindFinishedData()
        {
            if (_pldata == null) return;

            dgv_pl_finished.AutoGenerateColumns = false;

            var view = new DataView(_plTable);
            view.RowFilter = "status = 'COMPLETE'";

            dgv_pl_finished.DataSource = view;
            _finishedLoaded = true;
        }

        private void tab_container_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tab_container.SelectedIndex)
            {
                case 0: // Pending tab
                    if (!_pendingLoaded) BindPendingData();
                    break;
                case 1: // Ongoing tab
                    if (!_ongoingLoaded) BindOngoingData();
                    break;
                case 2: // Finished tab
                    if (!_finishedLoaded) BindFinishedData();
                    break;
            }
        }

        private void ApplyIncompleteToMaterials()
        {
            foreach (DataRow row in _plTable.Rows)
            {
                if (row["materials"] == DBNull.Value || string.IsNullOrWhiteSpace(row["materials"]?.ToString()))
                {
                    row["materials"] = "INCOMPLETE";
                }
            }
        }

        private void OpenUserControls(DataGridView dgv, string postfix, int rowIndex, bool hasMaterials = false, bool hasItemRqst = false)
        {
            if (rowIndex < 0) return;

            var row = dgv.Rows[rowIndex];
            var clickedColumn = dgv.Columns[dgv.CurrentCell.ColumnIndex].Name;

            if (hasMaterials && clickedColumn == $"materials_{postfix}")
            {
                var materialsCell = row.Cells[$"materials_{postfix}"];
                if (materialsCell?.Value?.ToString() != "INCOMPLETE") return;

                var bomIdCell = row.Cells[$"bom_id_{postfix}"];
                if (bomIdCell?.Value == null ||
                    string.IsNullOrWhiteSpace(bomIdCell.Value.ToString()) ||
                    bomIdCell.Value.ToString() == "0")
                {
                    Helpers.ShowDialogMessage("error", "No BOM ID found for this record.");
                    return;
                }

                using (var form = new MaterialsForm(bomIdCell.Value.ToString()))
                {
                    form.ShowDialog();
                }
            }
            else if (clickedColumn == $"sales_order_{postfix}")
            {
                var soIdCell = row.Cells[$"so_id_{postfix}"];
                if (soIdCell?.Value == null ||
                    string.IsNullOrWhiteSpace(soIdCell.Value.ToString()) ||
                    soIdCell.Value.ToString() == "0")
                {
                    Helpers.ShowDialogMessage("error", "No Sales Order ID found for this record.");
                    return;
                }

                var mainForm = this.FindForm() as SMPC;
                if (mainForm == null) return;

                mainForm.OpenRoute("Sales Order");

                foreach (Control ctrl in mainForm.tabContainer.SelectedTab.Controls)
                {
                    if (ctrl is Pages.SalesOrderEngineering.SalesOrderEngPage uc)
                    {
                        uc.SetSalesOrder(soIdCell.Value.ToString());
                        break;
                    }
                }
            }
            else if (hasItemRqst && clickedColumn == $"item_rqst_{postfix}")
            {
                var irIdCell = row.Cells[$"ir_id_{postfix}"];
                if (irIdCell?.Value == null ||
                    string.IsNullOrWhiteSpace(irIdCell.Value.ToString()) ||
                    irIdCell.Value.ToString() == "0")
                {
                    Helpers.ShowDialogMessage("error", "No Sales Order ID found for this record.");
                    return;
                }

                var mainForm = this.FindForm() as SMPC;
                if (mainForm == null) return;

                mainForm.OpenRoute("Item Request");

                foreach (Control ctrl in mainForm.tabContainer.SelectedTab.Controls)
                {
                    if (ctrl is Pages.ItemRequest.ItemRequestPage uc)
                    {
                        uc.SetItemRequest(irIdCell.Value.ToString());
                        break;
                    }
                }
            }
        }

        private void HandleReportFileUpload(int rowIndex)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select a Report File";
                openFileDialog.Filter = "Supported Files (*.pdf;*.xls;*.xlsx;*.doc;*.docx)|*.pdf;*.xls;*.xlsx;*.doc;*.docx";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                string filePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(filePath);

                try
                {
                    byte[] fileBytes = File.ReadAllBytes(filePath);
                    string base64String = Convert.ToBase64String(fileBytes);

                    var row = dgv_pl_finished.Rows[rowIndex];

                    // Set the display name in report_finished column
                    var reportCell = row.Cells["report_finished"];
                    if (reportCell != null)
                        reportCell.Value = fileName;

                    // Set the base64 string in report_base_finished column
                    var reportBaseCell = row.Cells["report_base_finished"];
                    if (reportBaseCell != null)
                        reportBaseCell.Value = base64String;

                    // Track this row as edited so it gets included in save
                    _editedRowIndices.Add(rowIndex);

                    Helpers.ShowDialogMessage("success", $"File \"{fileName}\" uploaded successfully.");
                }
                catch (Exception ex)
                {
                    Helpers.ShowDialogMessage("error", $"Failed to upload file: {ex.Message}");
                }
            }
        }

        private void tab_container_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (_isEditing)
            {
                e.Cancel = true;
                Helpers.ShowDialogMessage("error", "Please save or cancel your changes before switching tabs.");
            }
        }

        private void dgv_pl_pending_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_isEditing)
                return;

            OpenUserControls(dgv_pl_pending, "pending", e.RowIndex, hasMaterials: true);
        }

        private void dgv_pl_ongoing_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_isEditing)
                return;

            OpenUserControls(dgv_pl_ongoing, "ongoing", e.RowIndex, hasMaterials: true, hasItemRqst: true);
        }

        private void dgv_pl_finished_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var clickedColumn = dgv_pl_finished.Columns[e.ColumnIndex].Name;

            // Handle report upload ONLY when in edit mode
            if (clickedColumn == "report_finished")
            {
                if (_isEditing)
                    HandleReportFileUpload(e.RowIndex);
                return;
            }

            if (_isEditing)
                return;

            if (clickedColumn == "serial_no_finished")
            {
                var serialNoCell = dgv_pl_finished.Rows[e.RowIndex].Cells["serial_no_finished"];
                var serialNoValue = serialNoCell?.Value?.ToString();

                if (string.IsNullOrWhiteSpace(serialNoValue))
                {
                    Helpers.ShowDialogMessage("error", "No serial number found for this record.");
                    return;
                }

                var printDoc = new System.Drawing.Printing.PrintDocument();
                printDoc.PrintPage += (s, pe) =>
                {
                    pe.Graphics.DrawString(
                        serialNoValue,
                        new Font("Arial", 14, FontStyle.Bold),
                        Brushes.Black,
                        new PointF(100, 100)
                    );
                };

                using (var printDialog = new PrintDialog())
                {
                    printDialog.Document = printDoc;
                    if (printDialog.ShowDialog() == DialogResult.OK)
                    {
                        printDoc.Print();
                    }
                }

                return;
            }

            OpenUserControls(dgv_pl_finished, "finished", e.RowIndex, hasItemRqst: true);
        }

        private void Dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _isEditing)
                _editedRowIndices.Add(e.RowIndex);
        }

        private void txt_search_TextChanged(object sender, EventArgs e)
        {
            if (_plTable == null || _plTable.Rows.Count == 0)
                return;

            string searchText = txt_search.Text.Trim();

            DataGridView currentDgv;
            string rowFilter;

            switch (tab_container.SelectedIndex)
            {
                case 0:
                    currentDgv = dgv_pl_pending;
                    rowFilter = "(a_engr IS NULL OR a_engr = '') OR (due IS NULL OR due = '')";
                    break;
                case 1:
                    currentDgv = dgv_pl_ongoing;
                    rowFilter = "a_engr IS NOT NULL AND a_engr <> '' AND due IS NOT NULL AND due <> '' AND status <> 'COMPLETE'";
                    break;
                case 2:
                    currentDgv = dgv_pl_finished;
                    rowFilter = "status = 'COMPLETE'";
                    break;
                default:
                    return;
            }

            if (string.IsNullOrEmpty(searchText) || searchText == placeHolderText)
            {
                var view = new DataView(_plTable);
                view.RowFilter = rowFilter;
                currentDgv.DataSource = view;
            }
            else
            {
                // Use base column names (no postfix) since _plTable comes from Job Order Model
                var filteredView = new DataView(_plTable);
                filteredView.RowFilter = rowFilter;
                var filteredTable = filteredView.ToTable();

                var searchedData = Helpers.FilterDataTable(filteredTable, searchText,
                    "date", "sales_order", "general_name",
                    "item_desc", "type", "materials",
                    "due", "a_engr", "item_rqst",
                    "status", "serial_no", "report");
                currentDgv.DataSource = searchedData;
            }
        }

        private void dgv_pl_pending_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!_isEditing)
                return;

            if (e.RowIndex < 0 || !_isEditing) return;

            var dgv = (DataGridView)sender;
            var changedCol = dgv.Columns[e.ColumnIndex];

            if (changedCol.Name == "cmb_a_engr_pending")
            {
                var cmbCell = dgv.Rows[e.RowIndex].Cells["cmb_a_engr_pending"] as DataGridViewComboBoxCell;
                if (cmbCell?.Value == null) return;

                if (int.TryParse(cmbCell.Value.ToString(), out int selectedId))
                {
                    var engineer = _engrdata?.FirstOrDefault(x => x.id == selectedId);
                    if (engineer != null)
                    {
                        var aEngrCell = dgv.Rows[e.RowIndex].Cells["a_engr_pending"];
                        if (aEngrCell != null)
                            aEngrCell.Value = engineer.full_name;
                    }
                }
            }
            else if (changedCol.Name == "due_pending")
            {
                // DateCellPickerOverlay writes the cell's Value directly rather than
                // driving a normal BeginEdit/EndEdit session, so Dgv_CellEndEdit never
                // fires for it - mark the row edited here instead, or a picked due date
                // would silently get left out of what Save sends.
                _editedRowIndices.Add(e.RowIndex);
            }
        }

        private void dgv_pl_pending_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            var dgv = (DataGridView)sender;
            if (dgv.CurrentCell is DataGridViewComboBoxCell &&
                dgv.Columns[dgv.CurrentCell.ColumnIndex].Name == "cmb_a_engr_pending")
            {
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgv_pl_pending_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var dgv = (DataGridView)sender;
            var col = dgv.Columns[e.ColumnIndex];

            if (col.Name != "due_pending") return;

            var cell = dgv.Rows[e.RowIndex].Cells["due_pending"];
            if (cell?.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString())) return;

            string rawInput = cell.Value.ToString().Trim();

            string[] formats = {
                "M/d/yyyy", "MM/dd/yyyy",
                "M/d/yy",   "MM/dd/yy",
                "M-d-yyyy", "MM-dd-yyyy",
                "M.d.yyyy", "MM.dd.yyyy",
                "Mddyyyy",  "MMddyyyy",
                "yyyy-MM-dd"
            };

            if (DateTime.TryParseExact(rawInput, formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime parsedDate)
                || DateTime.TryParse(rawInput, out parsedDate))
            {
                cell.Value = parsedDate.ToString("MM/dd/yyyy");
            }
            else
            {
                cell.Value = string.Empty;
            }
        }
    }
}
