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
using smpc_engineering_app.Pages.ItemRequest.ItemRequestModals;
using smpc_engineering_app.Pages.Components;
using smpc_engineering_app.Services.Transaction;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Setup;

namespace smpc_engineering_app.Pages.ItemRequest
{
    public partial class ItemRequest : UserControl
    {
        //Dictionaries for the column grouping of datagridviews
        Dictionary<string, string[]> columnGroupsMain = new Dictionary<string, string[]>()
        {
            { "REQUEST", new string[] { "req_qty", "req_uom" } },
            { "ISSUED", new string[] { "issued_qty", "issued_uom" } },
        };

        readonly ItemRequestService itemRequestService = new ItemRequestService();
        readonly SalesOrderViewService salesOrderViewService = new SalesOrderViewService();
        private string userDepartment = CacheData.CurrentUser.department.ToLower();
        private readonly Panel[] _panels;
        private bool _isNewMode = false;
        private bool _isWarehouseUser;
        private bool _isEditMode = false;
        private int _currentIRIndex = -1;
        private int _previousIRIndex = -1;
        private List<ItemRequestModel> _itemRequests;
        private List<ItemRequestDetailsModel> _originalDetailsBackup;
        private ItemRequestList _irdata;
        private DataTable _sotable;
        private DataTable _irltable;
        private DataTable _irTable;
        private List<string> originalReqDeptItems;
        private DataTable _originalIRLBackup;
        private Dictionary<string, string> _originalTopValues;
        private Dictionary<string, string> _originalBotValues;
        private bool _isFilteredByRefDoc = false;
        private string _userName;

        public ItemRequest()
        {
            InitializeComponent();

            _isWarehouseUser = userDepartment == "warehouse";
            _userName = CacheData.CurrentUser.first_name + " " + CacheData.CurrentUser.last_name;
            btn_new.Visible = _isWarehouseUser ? false : true;
            btn_cancel.Visible = _isWarehouseUser ? false : true;
            _panels = new[] { pnl_top, pnl_bot };

            Helpers.EnableGroupHeaders(dgv_main, columnGroupsMain);
            Helpers.SetChildControlsEnabled(_panels, false, new string[] { });
        }

        private void SetEditableColumns(bool isEdit)
        {
            var editableColumns = !_isWarehouseUser
                ? new[] { "remarks", "req_qty" }
                : new[] { "remarks", "serial_no", "issued_qty" };

            foreach (var colName in editableColumns)
            {
                if (dgv_main.Columns.Contains(colName))
                    dgv_main.Columns[colName].ReadOnly = !isEdit;
            }
        }

        private void SetEditMode(bool enable, bool isNewMode = false)
        {
            SetEditableColumns(enable);
            dgv_main.AllowUserToAddRows = enable && !_isWarehouseUser;
            _isNewMode = isNewMode;
            _isEditMode = !isNewMode && enable;
            btn_forward.Enabled = enable;
            btn_cancel.Enabled = enable;

            var excludeControls = !_isWarehouseUser
                ? new[] { "txt_id", "txt_approved_by", "txt_issued_by", "txt_req_by", "txt_req_date", "txt_doc_no", "txt_received_by" }
                : new[] { "txt_required_date", "txt_id", "txt_approved_by", "txt_issued_by", "txt_req_by", "txt_req_date", "txt_doc_no" };

            if (enable && !isNewMode)
            {
                _originalTopValues = Helpers.BackupPanelData(pnl_top);
                _originalBotValues = Helpers.BackupPanelData(pnl_bot);
            }

            //Backup and clear irltable when entering edit/new mode
            if (isNewMode)
            {
                _originalIRLBackup = _irltable != null ? _irltable.Copy() : new DataTable();
                _irltable = new DataTable();
            }

            //Enable panels based on user role
            if (_isWarehouseUser)
            {
                // Warehouse user → only enable pnl_bot
                Helpers.SetChildControlsEnabled(new[] { pnl_bot }, enable, excludeControls);
                Helpers.SetChildControlsEnabled(new[] { pnl_top }, false, new string[] { }); // keep top disabled
            }
            else
            {
                // Non-warehouse users → enable both panels normally
                Helpers.SetChildControlsEnabled(_panels, enable, excludeControls);
            }

            Helpers.SetButtonVisibility(
                toolStrip1,
                visibleButtons: enable ? new[] { "btn_save", "btn_close" } : new[] { "btn_prev", "btn_next", "btn_search", "btn_edit", "btn_delete" },
                hiddenButtons: enable ? new[] { "btn_prev", "btn_next", "btn_search", "btn_edit", "btn_delete" } : new[] { "btn_save", "btn_close" }
            );

            //Hide btn_new entirely if warehouse user
            if (_isWarehouseUser)
            {
                btn_new.Visible = false;
                btn_cancel.Visible = false;
                btn_forward.Visible = false;
            }
            else
            {
                btn_new.Visible = !enable;
            }
        }

        private void ChangeRecord(int step)
        {
            if (_itemRequests == null || !_itemRequests.Any()) return;

            int newIndex = _currentIRIndex + step;
            if (newIndex >= 0 && newIndex < _itemRequests.Count)
            {

                cmb_ref_doc.SelectedIndex = -1;
                cmb_req_dept.SelectedIndex = -1;
                _currentIRIndex = newIndex;
                ShowCurrentRecord();
            }
        }

        private void btn_new_Click(object sender, EventArgs e)
        {
            btn_cancel.Visible = false;
            btn_forward.Visible = true;

            // Save current index before clearing
            _previousIRIndex = _currentIRIndex;
            SetEditMode(true, isNewMode: true);

            //Clear only the rows, keep columns
            dgv_main.DataSource = null;
            dgv_main.Rows.Clear();
            Helpers.ResetControls(_panels);
        }

        private void btn_edit_Click(object sender, EventArgs e)
        {
            // Backup the current record details before allowing edit
            var current = _itemRequests[_currentIRIndex];
            _originalDetailsBackup = _irdata.item_request_details
                .Where(d => d.ir_id == current.id)
                .Select(d => new ItemRequestDetailsModel
                {
                    id = d.id,
                    ir_id = d.ir_id,
                    item_id = d.item_id,
                    item_description = d.item_description,
                    req_qty = d.req_qty,
                    req_uom = d.req_uom,
                    issued_qty = d.issued_qty,
                    issued_uom = d.issued_uom,
                    remarks = d.remarks,
                    serial_no = d.serial_no,
                    total_req = d.total_req,
                    total_issued = d.total_issued
                })
                .ToList();

            SetEditMode(true);
        }

        private void btn_forward_Click(object sender, EventArgs e)
        {
            btn_forward.Visible = false;
            btn_cancel.Visible = true;
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            btn_forward.Visible = true;
            btn_cancel.Visible = false;
        }

        private async void btn_search_Click(object sender, EventArgs e)
        {
            if (_itemRequests == null || _itemRequests.Count == 0)
            {
                await LoadItemRequests();
            }

            using (var searchForm = new ItemRequestSearch())
            {
                if (searchForm.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(searchForm.SelectedIRId))
                {
                    if (int.TryParse(searchForm.SelectedIRId, out int selectedId))
                    {
                        int index = _itemRequests.FindIndex(r => r.id == selectedId);
                        if (index >= 0)
                        {
                            _currentIRIndex = index;
                            await LoadItemRequests();
                        }
                    }
                    else
                    {
                        Helpers.ShowDialogMessage("error", "Invalid record ID selected.");
                    }
                }
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

        private void btn_close_Click(object sender, EventArgs e)
        {
            bool wasNewMode = _isNewMode;
            bool wasEditMode = _isEditMode;

            SetEditMode(false);

            //If we were in new mode, restore the previous record
            if (wasNewMode && _previousIRIndex >= 0 && _itemRequests != null && _itemRequests.Count > 0)
            {
                _currentIRIndex = _previousIRIndex;
                ShowCurrentRecord();
            }

            //If we were in "edit" mode, reload the current record (discard new rows)
            else if (wasEditMode && _currentIRIndex >= 0 && _itemRequests != null && _itemRequests.Count > 0)
            {
                // Restore the original details from backup if available
                if (_originalDetailsBackup != null)
                {
                    var current = _itemRequests[_currentIRIndex];
                    // Remove modified details and replace with the original backup
                    _irdata.item_request_details.RemoveAll(d => d.ir_id == current.id);
                    _irdata.item_request_details.AddRange(_originalDetailsBackup);
                }

                //Restore irltable
                _irltable = _originalIRLBackup != null ? _originalIRLBackup.Copy() : new DataTable();

                ShowCurrentRecord(); // Rebinds original data from _irdata

                // Restore all textboxes, combo boxes, etc.
                Helpers.RestorePanelData(pnl_top, _originalTopValues);
                Helpers.RestorePanelData(pnl_bot, _originalBotValues);

                //Restore original binding if filtered by ref_doc
                if (_isFilteredByRefDoc)
                {
                    ShowCurrentRecord();
                    _isFilteredByRefDoc = false;
                }
            }
        }

        private async void btn_delete_Click(object sender, EventArgs e)
        {
            if (_currentIRIndex < 0)
            {
                Helpers.ShowDialogMessage("error", "No record selected to delete.");
                return;
            }

            var current = _itemRequests[_currentIRIndex];

            var confirm = MessageBox.Show($"Are you sure you want to delete Receiving Report #{current.id}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Helpers.Loading.ShowLoading(dgv_main, "Deleting data...");

                var irModel = new ItemRequestModel
                {
                    id = string.IsNullOrWhiteSpace(txt_id.Text) ? current.id : int.Parse(txt_id.Text),
                };

                var irPayload = new ItemRequestPayload
                {
                    item_request = irModel
                };

                await itemRequestService.DeleteIRRecord(irPayload);

                Helpers.ShowDialogMessage("success", "Receiving Report deleted successfully.");
            }
            catch(Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to delete: {ex.Message}");
            }
            finally
            {
                await LoadItemRequests();

                Helpers.Loading.HideLoading(dgv_main);
            }
        }

        private async void btn_save_Click(object sender, EventArgs e)
        {
            dgv_main.EndEdit();

            // Select panel based on department
            Panel panelToValidate = !_isWarehouseUser ? pnl_top : pnl_bot;

            // Validate required controls in selected panel
            bool hasError = Helpers.ValidateControlsValues(panelToValidate);

            if (hasError)
            {
                Helpers.ShowDialogMessage("error", "Please fill in all required fields.");
                return;
            }

            string[] columnsToValidate = _isWarehouseUser
                ? new[] { "issued_qty" }
                : new[] { "item_description", "req_qty" };

            if (await Helpers.ValidateDataGridViewCells(dgv_main, columnsToValidate))
                return;

            var itemRequestParent = Helpers.BuildModelFromPanels<ItemRequestModel>(_panels);
            bool isForward = !btn_forward.Visible;
            itemRequestParent.is_forward = isForward;
            var itemRequestDetails = Helpers.BuildModelsFromData<ItemRequestDetailsModel>(dgv_main);
            var itemRequestLocation = Helpers.BuildModelsFromData<ItemRequestLocationModel>(_irltable);

            //Validate department selection
            bool validDept = cmb_req_dept.Items.Cast<object>()
                .Any(item => item.ToString().Equals(itemRequestParent.req_dept, StringComparison.OrdinalIgnoreCase));

            if (!validDept)
            {
                Helpers.ShowDialogMessage("error", "Please select a valid department from the list.");
                return;
            }

            //Check if itemRequestDetails is null or empty
            if (itemRequestDetails == null || itemRequestDetails.Count == 0)
            {
                Helpers.ShowDialogMessage("error", "Please select at least one item.");
                return;
            }

            //Only set date and user info inside itemRequestParent when valid
            itemRequestParent.req_date = DateTime.Now.ToString("MM/dd/yyyy");

            if (_isWarehouseUser)
            {
                itemRequestParent.approved_by = _userName;
                itemRequestParent.issued_by = _userName;
            }
            else
            {
                itemRequestParent.req_by = _userName;
            }

            // Wrap everything into ReceivingReportPayload
            var irPayload = new ItemRequestPayload
            {
                item_request = itemRequestParent,
                item_request_details = itemRequestDetails,
                item_request_location = itemRequestLocation
            };

            try
            {
                Helpers.Loading.ShowLoading(dgv_main, "Saving data...");

                if (_isNewMode)
                {
                    var result = await itemRequestService.CreateIRRecord(irPayload);
                    Helpers.ShowDialogMessage("success", "Item Request created successfully.");
                }
                else
                {
                    var result = await itemRequestService.UpdateIRRecord(irPayload);
                    Helpers.ShowDialogMessage("success", "Item Request updated successfully.");
                }
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to save: {ex.Message}");
            }
            finally
            {
                SetEditMode(false);
                await LoadItemRequests();

                Helpers.Loading.HideLoading(dgv_main);
            }
        }

        private async void ItemRequest_Load(object sender, EventArgs e)
        {
            // store original items (from designer)
            originalReqDeptItems = cmb_req_dept.Items.Cast<string>().ToList();

            try
            {
                Helpers.Loading.ShowLoading(dgv_main, "Fetching data...");
                await LoadItemRequests();
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

        private async Task LoadItemRequests()
        {
            // save current index before reload
            int oldIndex = _currentIRIndex;

            //fill this declared value by the receiving reports data
            _irdata = await itemRequestService.GetAsModel();

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
                cmb_ref_doc.BeginUpdate();
                cmb_ref_doc.Items.Clear();
                cmb_ref_doc.Items.AddRange(uniqueRefDocs.ToArray());
                cmb_ref_doc.EndUpdate();
            }

            if (_irdata != null && _irdata.item_request != null && _irdata.item_request.Count > 0)
            {
                //Filter records if warehouse user
                if (_isWarehouseUser)
                {
                    _irdata.item_request = _irdata.item_request
                        .Where(r => r.is_forward == true)
                        .ToList();
                }

                //set this variable to the parent of the rr
                _itemRequests = _irdata.item_request;

                // restore old index if valid, otherwise fallback to 0
                if (oldIndex >= 0 && oldIndex < _itemRequests.Count)
                    _currentIRIndex = oldIndex;
                else
                    _currentIRIndex = 0;

                ShowCurrentRecord();
            }
            else
            {
                _itemRequests = new List<ItemRequestModel>();
                _currentIRIndex = -1;
                dgv_main.DataSource = null;
                btn_prev.Enabled = false;
                btn_next.Enabled = false;
            }
        }

        private void ShowCurrentRecord()
        {
            if (_currentIRIndex < 0 || _irdata == null || _irdata.item_request == null || !_irdata.item_request.Any())
                return;

            // Convert receiving report list to DataTable using helper
             _irTable = Helpers.ToDataTable(_irdata.item_request);
            _irltable = Helpers.ToDataTable(_irdata.item_request_location);

            //Clear and rebuild _irltable based on current record only
            var current = _itemRequests[_currentIRIndex];
            var filteredLocations = _irdata.item_request_location
                .Where(l => l.ir_id == current.id)
                .ToList();

            _irltable = filteredLocations.Any()
                ? Helpers.ToDataTable(filteredLocations)
                : new DataTable();

            if (_irTable.Rows.Count == 0 || _currentIRIndex >= _irTable.Rows.Count)
                return;

            cmb_req_dept.TextChanged -= cmb_req_dept_TextChanged;
            //Bind controls automatically (textboxes, checkboxes, etc.)
            Helpers.BindControls(_panels, _irTable, _currentIRIndex);
            cmb_req_dept.TextChanged += cmb_req_dept_TextChanged;

            if (_isWarehouseUser)
            {
                btn_cancel.Visible = btn_forward.Visible = false;
            }
            else
            {
                btn_cancel.Visible = current.is_forward == true;
                btn_forward.Visible = current.is_forward != true;
            }

            //Disable auto column generation before setting the data source
            dgv_main.AutoGenerateColumns = false;

            //Bind child details (grids)
            if (_irdata?.item_request_details != null)
            {
                var detailsForCurrent = new BindingList<ItemRequestDetailsModel>(_irdata.item_request_details.Where(d => d.ir_id == current.id).ToList());

                dgv_main.DataSource = detailsForCurrent;
            }
            else
            {
                dgv_main.DataSource = null;
            }

            //Enable/disable navigation buttons
            btn_prev.Enabled = _currentIRIndex > 0;
            btn_next.Enabled = _currentIRIndex < _itemRequests.Count - 1;
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
            string[] numericColumns = { "req_qty", "issued_qty" };

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

            dgv_main.AllowUserToAddRows = false;

            if (!_isWarehouseUser && columnName == "item_description")
            {
                using (var itemForm = new ItemRequestItems())
                {
                    if (itemForm.ShowDialog(this) == DialogResult.OK &&
                        !string.IsNullOrEmpty(itemForm.SelectedItemId))
                    {
                        currentRow.Cells["item_id"].Value = itemForm.SelectedItemId;
                        currentRow.Cells["item_description"].Value = itemForm.SelectedItemDesc;
                        currentRow.Cells["req_uom"].Value = itemForm.SelectedItemUom;
                    }
                }
                return;
            }
            else if (columnName == "issued_qty" && _isWarehouseUser)
            {
                // Safely get required values
                string currentUom = currentRow.Cells["req_uom"]?.Value?.ToString();
                if (!int.TryParse(currentRow.Cells["id"]?.Value?.ToString(), out int currentId))
                    return;

                // Filter rows by ir_details_id
                DataTable filteredIRLocation = _irltable.Clone();
                foreach (DataRow row in _irltable.Rows)
                {
                    if (row["ir_details_id"] != DBNull.Value &&
                        Convert.ToInt32(row["ir_details_id"]) == currentId)
                    {
                        filteredIRLocation.ImportRow(row);
                    }
                }

                // Show modal
                using (var qtyForm = new PickQtyModal
                {
                    PassedUom = currentUom,
                    PassedId = currentId,
                    PassedIRLocation = filteredIRLocation
                })
                {
                    if (qtyForm.ShowDialog() == DialogResult.OK)
                    {
                        int totalIssued = qtyForm.TotalIssuedQty;
                        currentRow.Cells["issued_qty"].Value = totalIssued;

                        if (totalIssued > 0)
                            currentRow.Cells["issued_uom"].Value = currentUom;

                        foreach (DataRow row in qtyForm.SelectedIssuedLocations.Rows)
                        {
                            // Clone structure if _irltable is empty
                            if (_irltable.Columns.Count == 0)
                                _irltable = qtyForm.SelectedIssuedLocations.Clone();

                            // Import each selected row to _irltable (append only)
                            _irltable.ImportRow(row);
                        }
                    }
                }
            }
        }

        private void dgv_main_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure valid cell
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            // Only trigger this for the 'req_qty' column
            string columnName = dgv_main.Columns[e.ColumnIndex].Name;
            if (columnName != "req_qty")
                return;

            // Get the current row being edited
            var currentRow = dgv_main.Rows[e.RowIndex];

            // Check if the req_qty value is not zero
            if (decimal.TryParse(currentRow.Cells["req_qty"].Value?.ToString(), out decimal reqQty))
            {
                // Only add a new row if req_qty is greater than 0 and this is the last row
                if (reqQty > 0 && e.RowIndex == dgv_main.Rows.Count - 1 && !_isWarehouseUser)
                {
                    // End current edit
                    dgv_main.EndEdit();

                    // Allow user to add rows (if disabled in edit mode)
                    dgv_main.AllowUserToAddRows = true;

                    // Get the bound list
                    if (dgv_main.DataSource is BindingList<ItemRequestDetailsModel> list)
                    {
                        list.Add(new ItemRequestDetailsModel());
                    }

                    // Prevent multiple unnecessary blank rows
                    dgv_main.AllowUserToAddRows = false;
                }
            }
        }

        private void cmb_req_dept_TextChanged(object sender, EventArgs e)
        {
            // store what user typed
            string text = cmb_req_dept.Text;
            int selStart = cmb_req_dept.SelectionStart;

            // filter from the original items (designer items)
            var filtered = originalReqDeptItems
                .Where(item => item.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            // prevent recursion
            cmb_req_dept.TextChanged -= cmb_req_dept_TextChanged;

            // Close dropdown first so .Text doesn’t trigger internal auto-select
            bool reopen = cmb_req_dept.DroppedDown;
            cmb_req_dept.DroppedDown = false;

            // rebuild list
            cmb_req_dept.BeginUpdate();
            cmb_req_dept.Items.Clear();
            cmb_req_dept.Items.AddRange(filtered.Cast<object>().ToArray());
            cmb_req_dept.EndUpdate();

            // restore the exact user text manually
            cmb_req_dept.Text = text;

            // safe cursor restore
            if (selStart < 0) selStart = 0;
            if (selStart > cmb_req_dept.Text.Length) selStart = cmb_req_dept.Text.Length;
            cmb_req_dept.SelectionStart = selStart;
            cmb_req_dept.SelectionLength = 0;

            // only show dropdown when user has typed something
            if (filtered.Count > 0 && !string.IsNullOrEmpty(text))
                cmb_req_dept.DroppedDown = true;

            // re-attach event
            cmb_req_dept.TextChanged += cmb_req_dept_TextChanged;
        }

        private void cmb_ref_doc_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get selected reference document
                string selectedRefDoc = cmb_ref_doc.SelectedItem?.ToString();
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

                // Convert filtered rows to DataTable
                DataTable filteredTable = filteredRows.CopyToDataTable();
                dgv_main.AutoGenerateColumns = false;

                // Bind the filtered DataTable to DataGridView
                dgv_main.DataSource = filteredTable;

                _isFilteredByRefDoc = true;
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load items for selected Ref Doc: {ex.Message}");
            }
        }
    }
}
