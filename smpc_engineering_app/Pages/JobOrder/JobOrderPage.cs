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

            // STATUS on the ongoing tab is a combo limited to ONGOING / FINISHED (§7.2),
            // but existing rows carry legacy values - tbl_trans_job_order.status holds
            // 'DONE' and '' as well - and a combo cell whose bound value is not among its
            // items raises DataError. Without this it pops the "value is not valid"
            // dialog on every affected row, repeatedly. Suppressed the same way the
            // pending grid already does: the cell simply renders blank until someone
            // picks a real value.
            dgv_pl_ongoing.DataError += (s, e) => e.ThrowException = false;
            dgv_pl_finished.DataError += (s, e) => e.ThrowException = false;

            // In constructor, replace CellValueChanged with CellEndEdit
            dgv_pl_pending.CellEndEdit += Dgv_CellEndEdit;
            dgv_pl_ongoing.CellEndEdit += Dgv_CellEndEdit;
            dgv_pl_finished.CellEndEdit += Dgv_CellEndEdit;

            // DataGridView has no built-in date-picker column - floats a real
            // DateTimePicker over due_pending's active cell instead of relying on
            // free-typed text (see DateCellPickerOverlay's own comment for why that
            // was also silently contributing to the "Due date is required" confusion).
            _duePickerOverlay = new DateCellPickerOverlay(dgv_pl_pending, "due_pending");

            // §2.5 assigns the prefix per document type and it carries the "#":
            // SO#, IREQ#. These passed "SO" with no "#", so the grid rendered
            // SO00000007 for a document the spec calls SO#0007.
            Helpers.DataGridViewDocumentFormatter.DataGridViewDocumentFormat(dgv_pl_pending, "sales_order_pending", "SO#");
            Helpers.DataGridViewDocumentFormatter.DataGridViewDocumentFormat(dgv_pl_ongoing, "sales_order_ongoing", "SO#");
            Helpers.DataGridViewDocumentFormatter.DataGridViewDocumentFormat(dgv_pl_finished, "sales_order_finished", "SO#");

            // ITEM REQUEST # was never formatted at all - it rendered ir.doc_no raw, so
            // an assigned request showed as "8750" rather than "IREQ#8750". A row with
            // no request yet formats to blank (see Dgv_CellFormatting), which is what
            // makes the empty cell mean "none raised" rather than "IREQ#0000".
            Helpers.DataGridViewDocumentFormatter.DataGridViewDocumentFormat(dgv_pl_pending, "item_rqst_pending", "IREQ#");
            Helpers.DataGridViewDocumentFormatter.DataGridViewDocumentFormat(dgv_pl_ongoing, "item_rqst_ongoing", "IREQ#");
            Helpers.DataGridViewDocumentFormatter.DataGridViewDocumentFormat(dgv_pl_finished, "item_rqst_finished", "IREQ#");

            // Render ITEM REQUEST # the same way SALES ORDER already renders - underlined
            // DodgerBlue - so an assigned request reads as the link it is. A job order with
            // no request yet formats to blank (Dgv_CellFormatting returns empty for 0), so
            // the styling shows nothing and the cell stays empty; clicking it still opens
            // Item Request so the user can raise one for that job order.
            StyleAsDocumentLink(dgv_pl_pending, "item_rqst_pending");
            StyleAsDocumentLink(dgv_pl_ongoing, "item_rqst_ongoing");
            StyleAsDocumentLink(dgv_pl_finished, "item_rqst_finished");
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

            // Neither REPORT nor SERIAL NUMBER/S goes through SetEditableColumns any more -
            // both stay ReadOnly and are filled by clicking, not typing. REPORT holds a file
            // name the picker sets, and SERIAL NUMBER/S holds one serial per unit, which a
            // single free-text cell cannot express (§10.7). They are styled per cell instead,
            // because what a click does changes with the mode and FreezeForEdit has just
            // rebound the grid and dropped the previous per-cell styles.
            StyleAllReportCells();
            StyleAllSerialCells();

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

            // Declared outside the try so the finally block can reach them. Column names
            // are stripped of their tab postfix below so the generic model mapper can
            // match them, and they MUST be put back - see RestoreColumnNames' call in
            // finally.
            var strippedColumns = new Dictionary<DataGridViewColumn, string>();

            void RestoreColumnNames()
            {
                foreach (var kvp in strippedColumns)
                {
                    kvp.Key.Name = kvp.Value;
                }
                strippedColumns.Clear();
            }

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
                foreach (DataGridViewColumn col in currentDgv.Columns)
                {
                    if (col.Name.EndsWith(postfix, StringComparison.OrdinalIgnoreCase))
                    {
                        string stripped = col.Name.Substring(0, col.Name.Length - postfix.Length);
                        strippedColumns[col] = col.Name;
                        col.Name = stripped;
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

                // Put the names back HERE, not only in finally.
                //
                // Stripping exists solely so BuildModelsFromData and the engr_id loop above
                // can match column names to model properties - both are done by this point,
                // and nothing below wants the stripped names. Leaving the restore to finally
                // meant the reload at the end of this try block (await LoadJobOrder ->
                // BindPendingData) ran while the columns were still renamed, so
                //     dgv_pl_pending.Columns["cmb_a_engr_pending"]
                // came back null and the cast to DataGridViewComboBoxColumn threw
                // NullReferenceException on every successful update (user-reported 2026-09-05).
                //
                // finally still calls this - it clears the dictionary, so the second call is
                // a no-op - which is what keeps the names from being left stripped if the API
                // call below throws.
                RestoreColumnNames();

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
                // Moved here from the success path: the column names are stripped of
                // their tab postfix before mapping, and anything thrown in between (the
                // API call, or the engr_id loop above) used to leave them stripped for
                // the lifetime of the screen - every later row.Cells["..._ongoing"]
                // lookup would then throw.
                RestoreColumnNames();

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
            view.RowFilter = "a_engr IS NOT NULL AND a_engr <> '' AND due IS NOT NULL AND due <> '' AND status <> 'FINISHED'";

            dgv_pl_ongoing.DataSource = view;
            _ongoingLoaded = true;
        }

        private void BindFinishedData()
        {
            if (_pldata == null) return;

            dgv_pl_finished.AutoGenerateColumns = false;

            // §7.2, production list: PENDING -> ONGOING -> FINISHED. This compared
            // against 'COMPLETE', which nothing writes - tbl_trans_job_order.status
            // holds 'FINISHED', 'DONE' and '' - so the FINISHED tab could never show a
            // single row, and because ONGOING is the negation of this test, completed
            // work stayed in ONGOING permanently.
            var view = new DataView(_plTable);
            view.RowFilter = "status = 'FINISHED'";

            dgv_pl_finished.DataSource = view;
            _finishedLoaded = true;

            // Per-cell styling does not survive a rebind, so re-apply it here rather than
            // only where a report or a serial is set.
            StyleAllReportCells();
            StyleAllSerialCells();
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

                // so_id, so the API can carve this job's own SO's own approved
                // reservation out of "stock held by others" (see sp_GetComponents.sql).
                // Same cell name pattern OpenUserControls' own sales_order branch
                // below already reads from this same row.
                var soIdCell = row.Cells[$"so_id_{postfix}"];
                string soId = soIdCell?.Value != null ? soIdCell.Value.ToString() : "0";

                using (var form = new MaterialsForm(bomIdCell.Value.ToString(), soId))
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
                bool hasItemRequest = irIdCell?.Value != null &&
                    !string.IsNullOrWhiteSpace(irIdCell.Value.ToString()) &&
                    irIdCell.Value.ToString() != "0";

                var mainForm = this.FindForm() as SMPC;
                if (mainForm == null) return;

                mainForm.OpenRoute("Item Request");

                foreach (Control ctrl in mainForm.tabContainer.SelectedTab.Controls)
                {
                    if (ctrl is Pages.ItemRequest.ItemRequestPage uc)
                    {
                        if (hasItemRequest)
                        {
                            // An Item Request already exists - open it to view.
                            uc.SetItemRequest(irIdCell.Value.ToString());
                        }
                        else
                        {
                            // No Item Request yet, which is the normal state for an
                            // ONGOING job whose materials are INCOMPLETE - the exact
                            // moment the user wants to raise one.
                            //
                            // This used to stop here with "No Sales Order ID found for
                            // this record." - a message copied from the SALES ORDER
                            // branch above, which was wrong twice over: it names the
                            // wrong field (the check is on ir_id, not so_id), and an
                            // absent Item Request is not an error condition at all.
                            // Open a new request with this job's Sales Order already
                            // selected instead (§5.9 step 1).
                            var soIdCell = row.Cells[$"so_id_{postfix}"];
                            uc.StartNewForSalesOrder(soIdCell?.Value?.ToString());
                        }
                        break;
                    }
                }
            }
        }

        // Matches sales_order_*'s DefaultCellStyle from the Designer (Underline +
        // DodgerBlue) rather than duplicating that styling into the Designer three more
        // times. Applied in code so the two link columns can never drift apart.
        private static void StyleAsDocumentLink(DataGridView dgv, string columnName)
        {
            if (dgv == null || !dgv.Columns.Contains(columnName)) return;

            var style = dgv.Columns[columnName].DefaultCellStyle;
            style.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Underline);
            style.ForeColor = Color.DodgerBlue;
        }

        // The REPORT column holds two values across two columns: report_finished carries the
        // file NAME the user sees, report_base_finished carries the payload. On the way up
        // report_base is base64 (UpdateJobOrder decodes it, writes it under ./files and
        // replaces the field with the stored filename); on the way back down it is that
        // filename. So a row whose report_base looks like a filename rather than base64 is a
        // report already sitting on the server - which is what OpenStoredReport fetches.
        private static bool IsStoredReportFilename(string reportBase)
        {
            if (string.IsNullOrWhiteSpace(reportBase)) return false;

            // UploadFile names files "<unix nanos><ext>" - short, single-segment, with an
            // extension. Base64 of any real document runs to thousands of characters and
            // carries no dot, so length alone separates them safely.
            return reportBase.Length < 260 && reportBase.Contains(".");
        }

        private static string ReportFileUrl(string storedFilename)
        {
            // app.Static("/files", "./files") in main.go serves these, and it hangs off the
            // server root rather than the /api group - so trim the /api suffix off the
            // configured base rather than assuming a separate setting exists for it.
            string apiBase = (Program.ApiBaseUrl ?? string.Empty).TrimEnd('/');
            if (apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
                apiBase = apiBase.Substring(0, apiBase.Length - 4);

            return $"{apiBase}/files/{storedFilename}";
        }

        // Downloads the stored report to a temp file and hands it to the shell. Nothing
        // read this back before: clicking REPORT outside edit mode fell straight through a
        // bare return, so a report could be attached and then never opened again
        // (user-reported 2026-09-05).
        private async Task OpenStoredReport(string storedFilename, string displayName)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_pl_finished, "Opening report...");

                // Keep the user's original file name where we have one - a report saved as
                // "1757... .pdf" tells them nothing about what they just opened.
                string safeName = string.IsNullOrWhiteSpace(displayName) ? storedFilename : displayName;
                foreach (char bad in Path.GetInvalidFileNameChars())
                    safeName = safeName.Replace(bad, '_');

                string targetDir = Path.Combine(Path.GetTempPath(), "smpc_job_order_reports");
                Directory.CreateDirectory(targetDir);
                string targetPath = Path.Combine(targetDir, safeName);

                using (var client = new System.Net.Http.HttpClient())
                {
                    // ngrok's browser-warning interstitial returns HTML instead of the file
                    // unless this header is present - the same reason ApiService sets it.
                    client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");

                    var bytes = await client.GetByteArrayAsync(ReportFileUrl(storedFilename));
                    File.WriteAllBytes(targetPath, bytes);
                }

                Process.Start(new ProcessStartInfo(targetPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Could not open the report: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_pl_finished);
            }
        }

        // The caption on an empty REPORT button. A saved report captions its own button with
        // its file name (the column binds to `report`, and UseColumnTextForButtonValue is
        // false - see the Designer), which is what keeps the file name on screen after a save
        // and makes it obvious a report exists. Only the empty case needs a caption supplied.
        private const string UploadButtonCaption = "Upload";

        private void dgv_pl_finished_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgv_pl_finished.Columns[e.ColumnIndex].Name != "report_finished") return;

            if (string.IsNullOrWhiteSpace(e.Value?.ToString()))
            {
                e.Value = UploadButtonCaption;
                e.FormattingApplied = true;
            }
        }

        // REPORT is a button column now, so the affordance is the button itself rather than
        // cell colouring. What is left per-row is the tooltip, which has to say different
        // things in the two modes - attach/replace while editing, open while viewing.
        private void StyleReportCell(DataGridViewRow row)
        {
            if (row == null || row.IsNewRow) return;
            if (!dgv_pl_finished.Columns.Contains("report_finished")) return;

            var cell = row.Cells["report_finished"];
            bool hasReport = !string.IsNullOrWhiteSpace(cell.Value?.ToString());

            cell.ToolTipText = _isEditing
                ? (hasReport
                    ? $"\"{cell.Value}\" is attached - click to replace or remove it"
                    : "Click to attach a report file")
                : (hasReport
                    ? $"Click to open \"{cell.Value}\""
                    : "No report attached - click Edit to attach one");
        }

        private void StyleAllReportCells()
        {
            if (dgv_pl_finished == null) return;

            foreach (DataGridViewRow row in dgv_pl_finished.Rows)
                StyleReportCell(row);
        }

        // SERIAL NUMBER/S carries a list, one entry per unit, so the cell shows how complete
        // that list is rather than just the raw joined string - "2 of 5" is the thing an
        // engineer actually needs to see at a glance on a row with QTY 5.
        private void StyleSerialCell(DataGridViewRow row)
        {
            if (row == null || row.IsNewRow) return;
            if (!dgv_pl_finished.Columns.Contains("serial_no_finished")) return;

            var cell = row.Cells["serial_no_finished"];

            int.TryParse(row.Cells["quantity_finished"]?.Value?.ToString(), out int quantity);
            if (quantity <= 0) quantity = 1;

            var serials = SerialNumberEntryModal.SplitSerials(cell.Value?.ToString());
            int filled = serials.Count(s => !string.IsNullOrWhiteSpace(s));

            cell.Style.BackColor = _isEditing ? Color.White : Color.Gainsboro;

            if (filled == 0)
            {
                cell.Style.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Italic);
                cell.Style.ForeColor = Color.Gray;
            }
            else
            {
                cell.Style.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Underline);
                cell.Style.ForeColor = Color.DodgerBlue;
            }

            cell.ToolTipText = _isEditing
                ? $"Click to enter serial numbers - {filled} of {quantity} unit{(quantity == 1 ? "" : "s")} recorded"
                : (filled > 0
                    ? $"Click to print {filled} serial number{(filled == 1 ? "" : "s")} for labelling"
                    : "No serial numbers recorded");
        }

        private void StyleAllSerialCells()
        {
            if (dgv_pl_finished == null) return;

            foreach (DataGridViewRow row in dgv_pl_finished.Rows)
                StyleSerialCell(row);
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

                    StyleReportCell(row);

                    // Not "uploaded" - nothing has left this machine yet. The file only
                    // reaches the server when Save posts report_base and UpdateJobOrder
                    // writes it under ./files. Saying "uploaded successfully" here told the
                    // user the job was done at the exact moment it was still possible to
                    // lose the attachment by clicking Cancel.
                    Helpers.ShowDialogMessage("success",
                        $"\"{fileName}\" attached. Click Save to upload it.");
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

            if (clickedColumn == "report_finished")
            {
                var reportRow = dgv_pl_finished.Rows[e.RowIndex];
                string storedBase = reportRow.Cells["report_base_finished"]?.Value?.ToString();
                string displayName = reportRow.Cells["report_finished"]?.Value?.ToString();

                if (_isEditing)
                {
                    // Nothing attached yet - go straight to the picker rather than making
                    // the user dismiss a prompt first.
                    if (string.IsNullOrWhiteSpace(storedBase))
                    {
                        HandleReportFileUpload(e.RowIndex);
                        return;
                    }

                    // Something is already attached. Replacing silently would throw away a
                    // report with no warning, and there was previously no way to detach one
                    // at all - so offer both. Yes = replace, No = remove, Cancel = leave it.
                    var choice = MessageBox.Show(
                        $"\"{displayName}\" is already attached to this job order." + Environment.NewLine + Environment.NewLine +
                        "Yes - replace it with a different file" + Environment.NewLine +
                        "No - remove it" + Environment.NewLine +
                        "Cancel - leave it as it is",
                        "Report",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button3);

                    if (choice == DialogResult.Yes)
                    {
                        HandleReportFileUpload(e.RowIndex);
                    }
                    else if (choice == DialogResult.No)
                    {
                        reportRow.Cells["report_finished"].Value = string.Empty;
                        reportRow.Cells["report_base_finished"].Value = string.Empty;
                        _editedRowIndices.Add(e.RowIndex);
                        StyleReportCell(reportRow);
                    }

                    return;
                }

                // Not editing: open what is attached. This branch used to be a bare return,
                // so an uploaded report was write-only.
                if (IsStoredReportFilename(storedBase))
                {
                    _ = OpenStoredReport(storedBase, displayName);
                }
                else if (!string.IsNullOrWhiteSpace(storedBase))
                {
                    // Attached in this editing session but not saved yet, so it exists only
                    // as base64 in the grid - there is nothing on the server to fetch.
                    Helpers.ShowDialogMessage("info",
                        $"\"{displayName}\" has not been saved yet. Click Save to upload it, then open it from here.");
                }
                else
                {
                    Helpers.ShowDialogMessage("info",
                        "No report is attached to this job order. Click Edit, then click this cell to attach one.");
                }

                return;
            }

            // SERIAL NUMBER/S is reachable in BOTH modes - entry while editing, printing
            // while viewing - so it is handled before the blanket "not editing" gate below.
            if (clickedColumn == "serial_no_finished")
            {
                if (_isEditing)
                {
                    HandleSerialNumberEntry(e.RowIndex);
                }
                else
                {
                    PrintSerialNumbers(e.RowIndex);
                }
                return;
            }

            if (_isEditing)
                return;

            OpenUserControls(dgv_pl_finished, "finished", e.RowIndex, hasItemRqst: true);
        }

        // Serial numbers are entered ONE PER UNIT (§10.7: "one per unit"), through a modal
        // carrying a line for each of the row's QTY units - not as one free-text blob typed
        // into the cell, which is what the column used to accept and which loses the
        // unit-to-serial mapping entirely.
        //
        // §5.23/§10.7 actually call for these to be GENERATED, not typed. That half is
        // deliberately not built: nothing in the spec defines a production serial's format
        // and inventing one is not a call to make here (user decision 2026-09-05 - typed for
        // now). The storage shape is already the one a generator would write into.
        private void HandleSerialNumberEntry(int rowIndex)
        {
            var row = dgv_pl_finished.Rows[rowIndex];
            if (row.IsNewRow) return;

            int.TryParse(row.Cells["quantity_finished"]?.Value?.ToString(), out int quantity);
            string existing = row.Cells["serial_no_finished"]?.Value?.ToString();
            string itemName = row.Cells["general_name_finished"]?.Value?.ToString();

            using (var modal = new SerialNumberEntryModal(quantity, existing, itemName))
            {
                if (modal.ShowDialog() != DialogResult.OK) return;

                row.Cells["serial_no_finished"].Value = modal.SerialNumbers;

                // The cell is ReadOnly, so Dgv_CellEndEdit never fires for it - mark the
                // row edited here or the serials would be left out of what Save sends.
                _editedRowIndices.Add(rowIndex);

                StyleSerialCell(row);
            }
        }

        // §5.23: serial numbers are "printable for labelling". One per line, so a sheet can
        // be cut into labels - the previous version printed the whole stored value as a
        // single string at a fixed (100, 100), which is not a label of anything.
        private void PrintSerialNumbers(int rowIndex)
        {
            var row = dgv_pl_finished.Rows[rowIndex];
            var serials = SerialNumberEntryModal
                .SplitSerials(row.Cells["serial_no_finished"]?.Value?.ToString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (serials.Count == 0)
            {
                Helpers.ShowDialogMessage("info",
                    "No serial numbers recorded for this job order. Click Edit, then click this cell to enter them.");
                return;
            }

            string itemName = row.Cells["general_name_finished"]?.Value?.ToString() ?? string.Empty;

            var printDoc = new System.Drawing.Printing.PrintDocument();
            int nextToPrint = 0;

            printDoc.PrintPage += (s, pe) =>
            {
                using (var labelFont = new Font("Arial", 14, FontStyle.Bold))
                using (var captionFont = new Font("Arial", 8, FontStyle.Regular))
                {
                    float y = pe.MarginBounds.Top;
                    float lineHeight = labelFont.GetHeight(pe.Graphics) + captionFont.GetHeight(pe.Graphics) + 18f;

                    while (nextToPrint < serials.Count && y + lineHeight < pe.MarginBounds.Bottom)
                    {
                        pe.Graphics.DrawString(serials[nextToPrint], labelFont, Brushes.Black,
                            pe.MarginBounds.Left, y);

                        pe.Graphics.DrawString(itemName, captionFont, Brushes.Black,
                            pe.MarginBounds.Left, y + labelFont.GetHeight(pe.Graphics) + 2f);

                        y += lineHeight;
                        nextToPrint++;
                    }

                    // More than one page's worth - keep going rather than silently dropping
                    // every serial past the first page.
                    pe.HasMorePages = nextToPrint < serials.Count;
                }
            };

            using (var printDialog = new PrintDialog())
            {
                printDialog.Document = printDoc;
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    nextToPrint = 0;
                    printDoc.Print();
                }
            }
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
                    rowFilter = "a_engr IS NOT NULL AND a_engr <> '' AND due IS NOT NULL AND due <> '' AND status <> 'FINISHED'";
                    break;
                case 2:
                    currentDgv = dgv_pl_finished;
                    rowFilter = "status = 'FINISHED'";
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
