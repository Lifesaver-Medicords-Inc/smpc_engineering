using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Services.Helpers;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Transaction;
using smpc_engineering_app.Services.Setup;
using smpc_engineering_app.Pages.Components;
using smpc_engineering_app.Pages.PickActivity.PickActivityModals;

namespace smpc_engineering_app.Pages.PickActivity
{
    public partial class PickActivity : UserControl
    {
        //Dictionaries for the column grouping of datagridviews
        Dictionary<string, string[]> columnGroupsMain = new Dictionary<string, string[]>()
        {
            { "QTY LEFT", new string[] { "left_qty", "left_uom" } },
            { "QTY TO PICK", new string[] { "pick_qty", "pick_uom"} },
            { "ACTUAL PICK QTY", new string[] { "actual_qty", "actual_uom" } },
        };

        readonly PickActivityService pickActivityService = new PickActivityService();
        readonly SalesOrderPAViewService salesOrderViewService = new SalesOrderPAViewService();
        private string userDepartment = CacheData.CurrentUser.department.ToLower();
        private bool _isNewMode = false;
        private bool _isWarehouseUser;
        private bool _isEditMode = false;
        private int _currentPAIndex = -1;
        private int _previousPAIndex = -1;
        private List<PickActivityModel> _pickActivities;
        private PickActivityList _padata;
        private DataTable _sotable;
        private DataTable _paltable;
        private DataTable _paTable;
        private string _userName;

        public PickActivity()
        {
            InitializeComponent();

            _isWarehouseUser = userDepartment == "warehouse";
            _userName = CacheData.CurrentUser.first_name + " " + CacheData.CurrentUser.last_name;
            btn_new.Visible = _isWarehouseUser ? false : true;

            Helpers.EnableGroupHeaders(dgv_main, columnGroupsMain);
            Helpers.SetChildControlsEnabled(new Panel[] { pnl_top }, false, new string[] { });
        }

        private void SetEditableColumns(bool isEdit)
        {
            var editableColumns = !_isWarehouseUser
                ? new[] { "pick_qty" }
                : new[] { "actual_qty", "bin_location" };

            foreach (var colName in editableColumns)
            {
                if (dgv_main.Columns.Contains(colName))
                    dgv_main.Columns[colName].ReadOnly = !isEdit;
            }
        }

        private void SetEditMode(bool enable, bool isNewMode = false)
        {
            SetEditableColumns(enable);
            _isNewMode = isNewMode;
            _isEditMode = !isNewMode && enable;

            //Enable panels based on user role
            if (!_isWarehouseUser)
                Helpers.SetChildControlsEnabled(new[] { pnl_top }, enable, new string[] { "txt_customer", "txt_code", "txt_prepared_by", "txt_picked_by", "txt_sales_person", "txt_doc_no" });

            Helpers.SetButtonVisibility(
                toolStrip1,
                visibleButtons: enable ? new[] { "btn_save", "btn_close" } : new[] { "btn_prev", "btn_next", "btn_search", "btn_edit", "btn_delete" },
                hiddenButtons: enable ? new[] { "btn_prev", "btn_next", "btn_search", "btn_edit", "btn_delete" } : new[] { "btn_save", "btn_close" }
            );

            //Hide btn_new entirely if warehouse user
            if (_isWarehouseUser)
            {
                btn_new.Visible = false;
            }
            else
            {
                btn_new.Visible = !enable;
            }

            //Only clear _paltable if it's New Mode
            if (isNewMode && _paltable != null)
            {
                _paltable.Clear();
            }
        }

        private void ChangeRecord(int step)
        {
            if (_pickActivities == null || !_pickActivities.Any()) return;

            int newIndex = _currentPAIndex + step;
            if (newIndex >= 0 && newIndex < _pickActivities.Count)
            {

                cmb_reference_so.SelectedIndex = -1;
                _currentPAIndex = newIndex;
                ShowCurrentRecord();
            }
        }

        private void btn_prev_Click(object sender, EventArgs e)
        {
            ChangeRecord(-1);
        }

        private void btn_next_Click(object sender, EventArgs e)
        {
            ChangeRecord(1);
        }

        private void btn_new_Click(object sender, EventArgs e)
        {
            // Save current index before clearing
            _previousPAIndex = _currentPAIndex;
            SetEditMode(true, isNewMode: true);

            //Clear only the rows, keep columns
            dgv_main.DataSource = null;
            dgv_main.Rows.Clear();
            Helpers.ResetControls(new Panel[] { pnl_top });
        }

        private void btn_edit_Click(object sender, EventArgs e)
        {
            if (_currentPAIndex < 0 || _pickActivities == null || !_pickActivities.Any())
            {
                Helpers.ShowDialogMessage("error", "No record selected to edit.");
                return;
            }

            // Store last viewed record index
            _previousPAIndex = _currentPAIndex;

            SetEditMode(true);
        }

        private async void btn_close_Click(object sender, EventArgs e)
        {
            SetEditMode(false);


            // Return to the previous record index if available
            if (_previousPAIndex >= 0 && _pickActivities != null && _pickActivities.Count > 0)
            {
                _currentPAIndex = _previousPAIndex;
                await LoadPickActivities();
            }
        }

        private async void btn_delete_Click(object sender, EventArgs e)
        {
            if (_currentPAIndex < 0)
            {
                Helpers.ShowDialogMessage("error", "No record selected to delete.");
                return;
            }

            var current = _pickActivities[_currentPAIndex];

            var confirm = MessageBox.Show($"Are you sure you want to delete Pick Activity #{current.id}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Helpers.Loading.ShowLoading(dgv_main, "Deleting data...");

                var paModel = new PickActivityModel
                {
                    id = string.IsNullOrWhiteSpace(txt_id.Text) ? current.id : int.Parse(txt_id.Text),
                };

                var paPayload = new PickActivityPayload
                {
                    pick_activity = paModel
                };

                await pickActivityService.DeletePARecord(paPayload);

                Helpers.ShowDialogMessage("success", "Pick Activity deleted successfully.");
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to delete: {ex.Message}");
            }
            finally
            {
                await LoadPickActivities();

                Helpers.Loading.HideLoading(dgv_main);
            }
        }

        private async void btn_save_Click(object sender, EventArgs e)
        {
            dgv_main.EndEdit();

            // Validate required controls in selected panel
            bool hasError = Helpers.ValidateControlsValues(pnl_top);

            if (hasError) // if validation failed
            {
                Helpers.ShowDialogMessage("error", "Please fill in all required fields.");
                return;
            }

            string[] columnsToValidate = _isWarehouseUser
                ? new[] { "actual_qty", "bin_location" }
                : new[] { "pick_qty"};

            if (await Helpers.ValidateDataGridViewCells(dgv_main, columnsToValidate))
                return;

            if (_paltable == null || _paltable.Columns.Count == 0)
            {
                _paltable = Helpers.ToDataTable(new List<PickActivityLocationModel>());
            }

            var pickActivityParent = Helpers.BuildModelFromPanels<PickActivityModel>(new Panel[] { pnl_top });
            var pickActivityDetails = Helpers.BuildModelsFromData<PickActivityDetailsModel>(dgv_main);
            var pickActivityLocation = Helpers.BuildModelsFromData<PickActivityLocationModel>(_paltable);

            //Check if pick activity Details is null or empty
            if (pickActivityDetails == null || pickActivityDetails.Count == 0)
            {
                Helpers.ShowDialogMessage("error", "Please select at least one item.");
                return;
            }

            foreach (DataGridViewRow row in dgv_main.Rows)
            {
                if (row.IsNewRow) continue;

                // Get pick_qty and left_qty as decimals (handle parsing safely)
                decimal pickQty = 0, leftQty = 0;
                decimal.TryParse(row.Cells["pick_qty"]?.Value?.ToString(), out pickQty);
                decimal.TryParse(row.Cells["left_qty"]?.Value?.ToString(), out leftQty);

                // Compare values
                if (pickQty > leftQty)
                {
                    string itemDesc = row.Cells["item_description"]?.Value?.ToString() ?? "Unknown Item";
                     Helpers.ShowDialogMessage("error",
                        $"Requested quantity for '{itemDesc}' cannot exceed the ordered quantity ({leftQty}).");
                     return;
                }
            }

            if (_isWarehouseUser)
            {
                pickActivityParent.picked_by = _userName;
            }
            else
            {
                pickActivityParent.prepared_by = _userName;
            }

            // Wrap everything into ReceivingReportPayload
            var paPayload = new PickActivityPayload
            {
                pick_activity = pickActivityParent,
                pick_activity_details = pickActivityDetails,
                pick_activity_location = pickActivityLocation
            };

            try
            {
                Helpers.Loading.ShowLoading(dgv_main, "Saving data...");

                if (_isNewMode)
                {
                    var result = await pickActivityService.CreatePARecord(paPayload);
                    Helpers.ShowDialogMessage("success", "Pick Activity created successfully.");
                }
                else
                {
                    var result = await pickActivityService.UpdatePARecord(paPayload);
                    Helpers.ShowDialogMessage("success", "Pick Activity updated successfully.");
                }
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to save: {ex.Message}");
            }
            finally
            {
                SetEditMode(false);
                await LoadPickActivities();

                Helpers.Loading.HideLoading(dgv_main);
            }
        }

        private async void btn_search_Click(object sender, EventArgs e)
        {
            if (_pickActivities == null || _pickActivities.Count == 0)
            {
                await LoadPickActivities();
            }

            using (var searchForm = new PickActivitySearch())
            {
                if (searchForm.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(searchForm.SelectedPAId))
                {
                    if (int.TryParse(searchForm.SelectedPAId, out int selectedId))
                    {
                        int index = _pickActivities.FindIndex(r => r.id == selectedId);
                        if (index >= 0)
                        {
                            _currentPAIndex = index;
                            await LoadPickActivities();
                        }
                    }
                    else
                    {
                        Helpers.ShowDialogMessage("error", "Invalid record ID selected.");
                    }
                }
            }
        }

        private async void PickActivity_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_main, "Fetching data...");
                await LoadPickActivities();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_main);
            }
        }

        private async Task LoadPickActivities()
        {
            // save current index before reload
            int oldIndex = _currentPAIndex;

            //fill this declared value by the receiving reports data
            _padata = await pickActivityService.GetAsModel();

            // Reverse order so newest records appear first
            _padata.pick_activity.Reverse();

            _sotable = await salesOrderViewService.GetAsDatatable();

            if (_sotable != null && _sotable.Rows.Count > 0 && _sotable.Columns.Contains("ref_doc"))
            {
                // Get unique non-empty ref_doc values
                var uniqueRefDocs = _sotable.AsEnumerable()
                    .Select(r => r.Field<string>("ref_doc"))
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct()
                    .OrderBy(v => v)
                    .ToList();

                // Bind to ComboBox
                cmb_reference_so.BeginUpdate();
                cmb_reference_so.Items.Clear();
                cmb_reference_so.Items.AddRange(uniqueRefDocs.ToArray());
                cmb_reference_so.EndUpdate();
            }

            if (_padata != null && _padata.pick_activity != null && _padata.pick_activity.Count > 0)
            {
                //set this variable to the parent of the rr
                _pickActivities = _padata.pick_activity;

                // restore old index if valid, otherwise fallback to 0
                if (oldIndex >= 0 && oldIndex < _pickActivities.Count)
                    _currentPAIndex = oldIndex;
                else
                    _currentPAIndex = 0;

                ShowCurrentRecord();
            }
            else
            {
                _pickActivities = new List<PickActivityModel>();
                _currentPAIndex = -1;
                dgv_main.DataSource = null;
                btn_prev.Enabled = false;
                btn_next.Enabled = false;
            }
        }

        private void ShowCurrentRecord()
        {
            if (_currentPAIndex < 0 || _padata == null || _padata.pick_activity == null || !_padata.pick_activity.Any())
                return;

            // Convert receiving report list to DataTable using helper
            _paTable = Helpers.ToDataTable(_padata.pick_activity);
            _paltable = Helpers.ToDataTable(_padata.pick_activity_location);

            //Clear and rebuild _irltable based on current record only
            var current = _pickActivities[_currentPAIndex];
            var filteredLocations = _padata.pick_activity_location
                .Where(l => l.pa_id == current.id)
                .ToList();

            _paltable = filteredLocations.Any()
                ? Helpers.ToDataTable(filteredLocations)
                : new DataTable();

            if (_paTable.Rows.Count == 0 || _currentPAIndex >= _paTable.Rows.Count)
                return;

            //Bind controls automatically (textboxes, checkboxes, etc.)
            Helpers.BindControls(new Panel[] { pnl_top }, _paTable, _currentPAIndex);

            //Disable auto column generation before setting the data source
            dgv_main.AutoGenerateColumns = false;

            //Bind child details (grids)
            if (_padata?.pick_activity_details != null)
            {
                var detailsForCurrent = new BindingList<PickActivityDetailsModel>(_padata.pick_activity_details.Where(d => d.pa_id == current.id).ToList());

                dgv_main.DataSource = detailsForCurrent;
            }
            else
            {
                dgv_main.DataSource = null;
            }

            //Enable/disable navigation buttons
            btn_prev.Enabled = _currentPAIndex > 0;
            btn_next.Enabled = _currentPAIndex < _pickActivities.Count - 1;
        }

        private void dgv_main_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;

            // Ensure the numbering column exists
            if (grid.Columns.Contains("number"))
            {
                grid.Rows[e.RowIndex].Cells["number"].Value = (e.RowIndex + 1).ToString();
            }
        }

        private void dgv_main_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // Get the name of the current column
            string columnName = dgv_main.Columns[dgv_main.CurrentCell.ColumnIndex].Name;

            // List of column names that should accept only numbers
            string[] numericColumns = { "pick_qty", "actual_qty" };

            // Check if the current column is one of them
            if (numericColumns.Contains(columnName))
            {
                // Remove any existing handler first (to avoid duplicates)
                e.Control.KeyPress -= new KeyPressEventHandler(NumericColumn_KeyPress);

                // Add our numeric-only handler
                e.Control.KeyPress += new KeyPressEventHandler(NumericColumn_KeyPress);
            }
            else
            {
                // Remove handler for all other columns
                e.Control.KeyPress -= new KeyPressEventHandler(NumericColumn_KeyPress);
            }
        }

        // Allow only numbers (and optional decimal point)
        private void NumericColumn_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow digits, control keys, and one decimal point
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true; // block
            }

            // Allow only one decimal point
            if (e.KeyChar == '.' && (sender as TextBox).Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void dgv_main_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore header and invalid clicks
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            // Determine mode and column name early
            bool isEditing = _isNewMode || _isEditMode;
            string columnName = dgv_main.Columns[e.ColumnIndex].Name;

            if (!isEditing)
                return;

            if (dgv_main.CurrentCell != null && dgv_main.IsHandleCreated && dgv_main.Enabled)
            {
                try
                {
                    dgv_main.BeginEdit(true);
                    dgv_main.NotifyCurrentCellDirty(true);
                }
                catch (InvalidOperationException)
                {
                    // Ignore if DataGridView is not ready for editing
                }
            }

            // Ensure DataGridView is not in edit mode
            dgv_main.EndEdit();

            var currentRow = dgv_main.Rows[e.RowIndex];

            if (columnName == "actual_qty" && _isWarehouseUser)
            {
                // Safely get required values
                string currentUom = currentRow.Cells["pick_uom"]?.Value?.ToString();
                int currentItemId = currentRow.Cells["item_id"]?.Value == null ? 0 : Convert.ToInt32(currentRow.Cells["item_id"].Value);
                if (!int.TryParse(currentRow.Cells["id"]?.Value?.ToString(), out int currentId))
                    return;

                // Filter rows by ir_details_id
                DataTable filteredPALocation = _paltable.Clone();
                foreach (DataRow row in _paltable.Rows)
                {
                    if (row["pa_details_id"] != DBNull.Value &&
                        Convert.ToInt32(row["pa_details_id"]) == currentId)
                    {
                        filteredPALocation.ImportRow(row);
                    }
                }

                // Show modal
                using (var qtyForm = new PickQtyModal
                {
                    PassedItemId = currentItemId,
                    PassedUom = currentUom,
                    PassedPAId = currentId,
                    PassedPALocation = filteredPALocation
                })
                {
                    if (qtyForm.ShowDialog() == DialogResult.OK)
                    {
                        int totalIssued = qtyForm.TotalIssuedQty;
                        currentRow.Cells["actual_qty"].Value = totalIssued;

                        if (totalIssued > 0)
                            currentRow.Cells["actual_uom"].Value = currentUom;

                        foreach (DataRow row in qtyForm.SelectedIssuedLocations.Rows)
                        {
                            // Clone structure if _irltable is empty
                            if (_paltable.Columns.Count == 0)
                                _paltable = qtyForm.SelectedIssuedLocations.Clone();

                            // Import each selected row to _irltable (append only)
                            _paltable.ImportRow(row);
                        }
                    }
                }
            }
        }

        private void cmb_reference_so_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_isEditMode && !_isNewMode)
                    return;

                string selectedRefDoc = cmb_reference_so.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedRefDoc) || _sotable == null)
                {
                    dgv_main.DataSource = null;
                    dgv_main.Rows.Clear();
                    return;
                }

                // Filter rows from _sotable based on selected ref_doc
                var filteredRows = _sotable.AsEnumerable()
                    .Where(r => r.Field<string>("ref_doc") == selectedRefDoc)
                    .ToList();

                if (filteredRows.Count == 0)
                {
                    dgv_main.DataSource = null;
                    dgv_main.Rows.Clear();
                    return;
                }

                //Get the customer from the first matching row
                string customerName = filteredRows.First().Field<string>("customer");
                string customercode = filteredRows.First().Field<string>("code");
                string salesPerson = filteredRows.First().Field<string>("sales_person");
                txt_customer.Text = customerName;
                txt_code.Text = customercode;
                txt_sales_person.Text = salesPerson;

                // Convert filtered rows to DataTable
                DataTable filteredTable = filteredRows.CopyToDataTable();
                dgv_main.AutoGenerateColumns = false;

                // Bind the filtered DataTable to DataGridView
                dgv_main.DataSource = filteredTable;
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load items for selected Reference Doc: {ex.Message}");
            }
        }
    }
}
