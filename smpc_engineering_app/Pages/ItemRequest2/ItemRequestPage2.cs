using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Services.Transaction;
using smpc_engineering_app.Models;
using smpc_engineering_app.Services;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Helpers;
using smpc_engineering_app.Pages.ItemRequest.ItemRequestModals;
using smpc_engineering_app.Pages.Components;

namespace smpc_engineering_app.Pages.ItemRequest2
{
    public partial class ItemRequestPage2 : UserControl
    {
        ItemRequestService2 itemRequestService = new ItemRequestService2();
        private int _currentIRIndex = -1;
        private int _previousIRIndex = -1;
        private ItemRequestList2 _irdata;
        private List<ItemRequest2Model> _itemRequests;
        private List<UserListViewModel> _userdata;
        private List<ItemRequestSalesOrderDocView> _salesDocdata;
        private List<ItemRequestSalesOrderView> _salesdata;
        private DataTable _irTable;
        private BindingList<ItemRequestDetails2Model> _currentDetails;
        private bool _isNewMode = false;
        private bool _isEditMode = false;
        private bool _isEditing = false;
        private readonly Panel[] _panels;
        GeneralService<UserListViewModel> userListServiceSetup;
        GeneralService<ItemRequestSalesOrderDocView> salesOrderDocServiceSetup;
        GeneralService<ItemRequestSalesOrderView> salesOrderServiceSetup;
        private int _sales_order_id;
        private bool _isForward;
        private string _userDepartment = CacheData.CurrentUser.department.ToLower();
        private string _userName = CacheData.CurrentUser.first_name + " " + CacheData.CurrentUser.last_name;
        private bool _isWarehouse;
        private BindingList<ItemRequestLocation2Model> _currentLocations = new BindingList<ItemRequestLocation2Model>();

        //Dictionaries for the column grouping of datagridviews
        Dictionary<string, string[]> columnGroupsMain = new Dictionary<string, string[]>()
        {
            { "REQUEST", new string[] { "required_qty", "required_uom" } },
            { "ISSUED", new string[] { "issued_qty", "issued_uom" } },
            { "REMAINING", new string[] { "remaining_qty", "remaining_uom" } },
        };

        public ItemRequestPage2()
        {
            InitializeComponent();

            Helpers.EnableGroupHeaders(dgv_main, columnGroupsMain);
            _panels = new[] { pnl_top, pnl_bottom };
        }

        private void SetEditableColumns(bool isEdit)
        {
            var alwaysEditableColumns = _isWarehouse ? Array.Empty<string>() : new[] { "required_qty" };
            var newModeOnlyColumns = new[] { "required_qty" };

            foreach (var colName in alwaysEditableColumns)
            {
                if (dgv_main.Columns.Contains(colName))
                {
                    var column = dgv_main.Columns[colName];

                    // received_qty is editable ONLY in new mode, readonly in edit mode
                    if (newModeOnlyColumns.Contains(colName))
                        column.ReadOnly = !isEdit || _isEditMode;
                    else
                        column.ReadOnly = !isEdit;

                    column.DefaultCellStyle.BackColor = column.ReadOnly ? Color.Gainsboro : Color.White;
                }
            }

            ApplyWarehouseRestrictions(isEdit);
        }

        private void ApplyWarehouseRestrictions(bool isEditMode = false)
        {
            if (!_isWarehouse) return;

            // Hide new and delete buttons
            if (toolStrip1.Items.ContainsKey("btn_new"))
                toolStrip1.Items["btn_new"].Visible = false;
            if (toolStrip1.Items.ContainsKey("btn_delete"))
                toolStrip1.Items["btn_delete"].Visible = false;

            // Prevent warehouse users from adding new rows entirely
            dgv_main.AllowUserToAddRows = false;

            // Enable cmb_received_by only when in edit mode
            bool receivedByEditable = isEditMode;
            cmb_received_by.Enabled = receivedByEditable;
            cmb_received_by.DropDownStyle = receivedByEditable ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown;
            cmb_received_by.BackColor = receivedByEditable ? Color.White : Color.FromArgb(235, 235, 235);

            // Mark cmb_received_by as required only for warehouse users in edit mode
            cmb_received_by.Tag = (isEditMode) ? "REQUIRED" : null;

            // Keep serial_no, remarks editable in edit mode
            string[] warehouseEditableColumns = { "serial_no", "remarks" };
            foreach (var colName in warehouseEditableColumns)
            {
                if (dgv_main.Columns.Contains(colName))
                {
                    var column = dgv_main.Columns[colName];
                    column.ReadOnly = !isEditMode;
                    column.DefaultCellStyle.BackColor = isEditMode ? Color.White : Color.Gainsboro;
                }
            }

            if (_isWarehouse)
            {
                btn_forward.Visible = false;
                btn_cancel.Visible = false;
            }

            cmb_requesting_dept.Enabled = false;
            cmb_requesting_dept.BackColor = Color.FromArgb(235, 235, 235);
            cmb_requesting_dept.DropDownStyle = ComboBoxStyle.DropDown;

            txt_purpose.Enabled = false;
            txt_purpose.ReadOnly = true;
            txt_purpose.BackColor = Color.FromArgb(235, 235, 235);

            dtp_issue_date.Enabled = false;
            dtp_required_date.Enabled = false;
        }

        private async Task SetEditMode(bool enable, bool isNewMode = false)
        {
            _isNewMode = isNewMode;
            _isEditing = enable;
            _isEditMode = enable && !isNewMode;

            SetEditableColumns(enable);
            dgv_main.AllowUserToAddRows = enable;

            // buttons
            string[] editButtons = { "btn_save", "btn_close", "btn_forward", "btn_cancel" };
            string[] navButtons = { "btn_new", "btn_print", "btn_edit", "btn_delete", "btn_next", "btn_prev", "btn_search" };

            Helpers.SetButtonVisibility2(
                toolStrip1,
                pnl_bottom,
                visibleButtons: enable ? editButtons : navButtons,
                hiddenButtons: enable ? navButtons : editButtons
            );

            Helpers.SetChildControlsEnabled(_panels, !enable, new string[] { "txt_doc_no", "dtp_request_date",
                "txt_requested_by", "cmb_received_by", "txt_approved_by", "txt_issued_by" });

            // Manually control cmb_ref_doc: enabled only in new mode, disabled in edit mode
            cmb_ref_doc.Enabled = enable && _isNewMode;
            cmb_ref_doc.DropDownStyle = (enable && _isNewMode) ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown;
            cmb_ref_doc.BackColor = (enable && _isNewMode) ? Color.White : Color.FromArgb(235, 235, 235);

            // Load and bind combos only when entering edit or new mode
            if (enable)
            {
                await LoadSalesOrderDoc();
                await LoadUserList();

                cmb_ref_doc.SelectedIndexChanged -= cmb_ref_doc_SelectedIndexChanged;
                cmb_received_by.SelectedIndexChanged -= cmb_received_by_SelectedIndexChanged;

                if (_salesDocdata != null)
                {
                    cmb_ref_doc.DataSource = _salesDocdata;
                    cmb_ref_doc.DisplayMember = "so_doc_no";
                    cmb_ref_doc.ValueMember = "sales_order_id";

                    // §2.5: a Sales Order reads SO#0001 everywhere it is shown. Display only -
                    // SelectedItem/SelectedValue still hand back the raw so_doc_no and
                    // sales_order_id that cmb_ref_doc_SelectedIndexChanged reads.
                    Helpers.ComboBoxDocumentFormatter.ComboBoxDocumentFormat(cmb_ref_doc, "SO#");
                }

                if (_isEditMode)
                {
                    cmb_ref_doc.SelectedIndex = -1;

                    // txt_ref_doc holds the raw stored value, so prefix it here too - without
                    // this the closed box reads 0001 while the open dropdown reads SO#0001.
                    cmb_ref_doc.Text = Helpers.ComboBoxDocumentFormatter.FormatDocumentNo("SO#", txt_ref_doc.Text);
                }

                if (_userdata != null)
                {
                    cmb_received_by.DataSource = _userdata;
                    cmb_received_by.DisplayMember = "user_name";
                    cmb_received_by.ValueMember = "user_id";
                }

                cmb_ref_doc.SelectedIndexChanged += cmb_ref_doc_SelectedIndexChanged;
                cmb_received_by.SelectedIndexChanged += cmb_received_by_SelectedIndexChanged;
            }
            else
            {
                // Clear combos when leaving edit mode
                cmb_ref_doc.DataSource = null;
                cmb_received_by.DataSource = null;
            }

            ApplyWarehouseRestrictions(enable);
        }

        private void ChangeRecord(int step)
        {
            if (_itemRequests == null || !_itemRequests.Any()) return;

            int newIndex = _currentIRIndex + step;
            if (newIndex >= 0 && newIndex < _itemRequests.Count)
            {
                _currentIRIndex = newIndex;
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

        private async void btn_close_Click(object sender, EventArgs e)
        {
            await SetEditMode(false);

            // If no records exist, clear everything
            if (_itemRequests == null || !_itemRequests.Any())
            {
                ClearItemRequestUI();
                return;
            }

            // Return to the previous record index if available
            if (_previousIRIndex >= 0 && _itemRequests != null && _itemRequests.Count > 0)
            {
                _currentIRIndex = _previousIRIndex;
                await LoadItemRequests();
            }
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

        private async void btn_new_Click(object sender, EventArgs e)
        {
            // Save current index before clearing
            _previousIRIndex = _currentIRIndex;
            await SetEditMode(true, isNewMode: true);

            //Clear only the rows, keep columns
            _currentDetails = new BindingList<ItemRequestDetails2Model>();
            dgv_main.AutoGenerateColumns = false;
            dgv_main.DataSource = _currentDetails;
            Helpers.ResetControls(_panels);
        }

        private async void btn_edit_Click(object sender, EventArgs e)
        {
            if (_currentIRIndex < 0 || _itemRequests == null || !_itemRequests.Any())
            {
                Helpers.ShowDialogMessage("error", "No record selected to edit.");
                return;
            }

            // Store last viewed record index
            _previousIRIndex = _currentIRIndex;

            await SetEditMode(true);
        }

        private async void btn_delete_Click(object sender, EventArgs e)
        {
            if (_currentIRIndex < 0)
            {
                Helpers.ShowDialogMessage("error", "No record selected to delete.");
                return;
            }

            var current = _itemRequests[_currentIRIndex];

            var confirm = MessageBox.Show($"Are you sure you want to delete Item Request #{current.id}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                Helpers.Loading.ShowLoading(dgv_main, "Deleting data...");

                var itemRequestParent = Helpers.BuildModelFromPanels<ItemRequest2Model>(_panels);
                var itemRequestDetails = Helpers.DatagridviewMapper.BuildModelsFromData<ItemRequestDetails2Model>(dgv_main);
                var itemRequestLocations = _currentLocations?.ToList() ?? new List<ItemRequestLocation2Model>();

                var irPayload = new ItemRequestPayload2
                {
                    item_request = itemRequestParent,
                    item_request_details = itemRequestDetails,
                    item_request_locations = itemRequestLocations
                };

                var result = await itemRequestService.DeleteItemRequest(irPayload);

                if (!result.success)
                {
                    Helpers.ShowDialogMessage("error", "Item Request not deleted.");
                    return;
                }

                Helpers.ShowDialogMessage("success", "Item Request deleted successfully.");
            }
            catch (Exception ex)
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
            btn_save.Enabled = false;
            btn_cancel.Enabled = false;

            try
            {
                dgv_main.EndEdit();

                if (Helpers.ValidateControlsValues(_panels))
                {
                    Helpers.ShowDialogMessage("error", "Please fill in all required fields.");
                    return;
                }

                // ITEM DESCRIPTION is no longer required (user decision, 2026-09-05).
                //
                // This used to run Helpers.ValidateDataGridViewCells(dgv_main,
                // { "item_description" }) and refuse to save with "Please ensure all
                // required fields are filled", painting every blank description cell red.
                // A request built from a BOM legitimately has rows whose description
                // hasn't been filled in - they are identified by the item behind them, not
                // by typed text - so that blocked a perfectly valid request.
                //
                // item_description was the ONLY column in that list, so the whole call is
                // gone rather than left with an empty array. If a column ever does need
                // per-cell validation on this grid, this is where it goes back.
                //
                // The rest of the save's validation is untouched: ValidateControlsValues
                // above still requires the header fields (REQUESTING DEPT, PURPOSE, the
                // dates), and ValidateItemReqDetails below still enforces the quantity
                // rules - at least one required qty, and required not exceeding remaining.

                // Validate item request detail quantities
                if (!ValidateItemReqDetails())
                    return;

                var itemRequestParent = Helpers.BuildModelFromPanels<ItemRequest2Model>(_panels);

                itemRequestParent.requested_by = _userName;

                if (!_isWarehouse)
                {
                    itemRequestParent.is_forward = _isForward;
                }

                if (_isWarehouse)
                {
                    itemRequestParent.approved_by = _userName;
                    itemRequestParent.issued_by = _userName;
                }

                //Validate that item request details are not empty
                var itemRequestDetails = Helpers.DatagridviewMapper.BuildModelsFromData<ItemRequestDetails2Model>(dgv_main);
                var itemRequestLocations = _currentLocations?.ToList() ?? new List<ItemRequestLocation2Model>();

                // Set has_issued = true if issued_qty > 0
                foreach (var detail in itemRequestDetails)
                {
                    if (detail.issued_qty > 0)
                    {
                        detail.has_issued = true;
                    }
                    else
                    {
                        detail.has_issued = false;
                    }
                }

                if (itemRequestDetails == null || itemRequestDetails.Count == 0)
                {
                    Helpers.ShowDialogMessage("error", "Item Request cannot be empty.");
                    return;
                }

                // Wrap everything into Item Request Payload
                var irPayload = new ItemRequestPayload2
                {
                    item_request = itemRequestParent,
                    item_request_details = itemRequestDetails,
                    item_request_locations = itemRequestLocations
                };

                Helpers.Loading.ShowLoading(dgv_main, "Saving data...");

                if (_isNewMode)
                {
                    var result = await itemRequestService.CreateItemRequest(irPayload);

                    if (!result.success)
                    {
                        Helpers.ShowDialogMessage("error", "Item Request not created.");
                        return;
                    }

                    Helpers.ShowDialogMessage("success", "Item Request created successfully.");
                }
                else
                {
                    var result = await itemRequestService.UpdateItemRequest(irPayload);

                    if (!result.success)
                    {
                        Helpers.ShowDialogMessage("error", "Item Request not updated.");
                        return;
                    }

                    Helpers.ShowDialogMessage("success", "Item Request updated successfully.");
                }

                await SetEditMode(false);
                await LoadItemRequests();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to save: {ex.Message}");
            }
            finally
            {
                btn_save.Enabled = true;
                btn_cancel.Enabled = true;

                Helpers.Loading.HideLoading(dgv_main);
            }
        }

        private bool ValidateItemReqDetails()
        {
            if (_isEditMode && !_isWarehouse)
                return true;

            bool hasAtLeastOneRequired = false;
            bool hasAtLeastOneIssued = false;

            for (int i = 0; i < dgv_main.Rows.Count; i++)
            {
                var row = dgv_main.Rows[i];

                // Get cell values
                var requiredQtyVal = row.Cells["required_qty"].Value;
                var issuedQtyVal = row.Cells["issued_qty"].Value;
                var remainingQtyVal = row.Cells["remaining_qty"].Value;
                var serialVal = row.Cells["serial_no"].Value;

                decimal requiredQty = 0, issuedQty = 0, remainingQty = 0;

                decimal.TryParse(requiredQtyVal?.ToString(), out requiredQty);
                decimal.TryParse(issuedQtyVal?.ToString(), out issuedQty);
                decimal.TryParse(remainingQtyVal?.ToString(), out remainingQty);

                // Check if at least one row has required qty
                if (requiredQty > 0)
                    hasAtLeastOneRequired = true;

                // Check if at least one row has issued qty
                if (issuedQty > 0)
                    hasAtLeastOneIssued = true;

                // Validate: required must not exceed remaining.
                // Rows sourced from ItemRequestItems (no sales_order_details_id) are exempt —
                // they have no parent sales order line to constrain against.
                if (!_isWarehouse && !(_isEditMode && !_isWarehouse))
                {
                    var salesOrderDetailsId = row.Cells["sales_order_details_id"].Value;
                    bool isFromRefDoc = salesOrderDetailsId != null
                        && salesOrderDetailsId != DBNull.Value
                        && !string.IsNullOrEmpty(salesOrderDetailsId.ToString())
                        && salesOrderDetailsId.ToString() != "0";

                    if (isFromRefDoc && requiredQty > remainingQty)
                    {
                        string itemDesc = row.Cells["item_description"].Value?.ToString() ?? $"Row {i + 1}";
                        Helpers.ShowDialogMessage("error",
                            $"Row {i + 1} ({itemDesc}): The required ({requiredQty}) quantity exceeds the remaining quantity ({remainingQty}).");
                        return false;
                    }
                }

                // Validate: issued must not exceed required
                if (issuedQty > requiredQty)
                {
                    string itemDesc = row.Cells["item_description"].Value?.ToString() ?? $"Row {i + 1}";
                    Helpers.ShowDialogMessage("error",
                        $"Row {i + 1} ({itemDesc}): The issued ({issuedQty}) quantity exceeds the required quantity ({requiredQty}).");
                    return false;
                }

                // Validate: issued qty requires serial no
                if (issuedQty > 0)
                {
                    bool hasSerial = serialVal != null
                        && !string.IsNullOrWhiteSpace(serialVal.ToString());

                    if (!hasSerial)
                    {
                        string itemDesc = row.Cells["item_description"].Value?.ToString() ?? $"Row {i + 1}";
                        Helpers.ShowDialogMessage("error",
                            $"Row {i + 1} ({itemDesc}): serial number is required when a issued quantity is entered.");
                        return false;
                    }
                }
            }

            // Validate: at least one row must have a required qty
            if (!hasAtLeastOneRequired)
            {
                Helpers.ShowDialogMessage("error", "At least one item must have a required quantity greater than zero.");
                return false;
            }

            if (_isWarehouse)
            {
                // Validate: at least one row must have a issued qty
                if (!hasAtLeastOneIssued)
                {
                    Helpers.ShowDialogMessage("error", "At least one item must have a issued quantity greater than zero.");
                    return false;
                }

            }

            return true;
        }

        private async void ItemRequestPage2_Load(object sender, EventArgs e)
        {
            try
            {
                _isWarehouse = _userDepartment == "warehouse";

                Helpers.Loading.ShowLoading(dgv_main, "Fetching data...");
                await LoadItemRequests();

                ApplyWarehouseRestrictions();
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

            //fill this declared value by the Item Request data
            _irdata = await itemRequestService.GetAsModel();

            if (_irdata != null && _irdata.item_request != null && _irdata.item_request.Count > 0)
            {
                //Filter records if warehouse user
                if (_isWarehouse)
                {
                    _irdata.item_request = _irdata.item_request
                        .Where(r => r.is_forward == true)
                        .ToList();
                }

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
                ClearItemRequestUI();
            }
        }

        private void ShowCurrentRecord()
        {
            if (_currentIRIndex < 0 || _irdata == null || _irdata.item_request == null || !_irdata.item_request.Any())
                return;

            // Convert item request list to DataTable using helper
            _irTable = Helpers.ToDataTable(_irdata.item_request);

            Helpers.BindControls(_panels, _irTable, _currentIRIndex);

            //Disable auto column generation before setting the data source
            dgv_main.AutoGenerateColumns = false;

            var current = _itemRequests[_currentIRIndex];

            //Bind child details (grids)
            if (_irdata?.item_request_details != null)
            {
                _currentDetails = new BindingList<ItemRequestDetails2Model>(
                    _irdata.item_request_details
                        .Where(d => d.item_request_id == current.id)
                        .ToList()
                );

                dgv_main.DataSource = _currentDetails;

                //Bind child locations
                if (_irdata?.item_request_locations != null)
                {
                    _currentLocations = new BindingList<ItemRequestLocation2Model>(
                        _irdata.item_request_locations
                            .Where(d => d.item_request_id == current.id)
                            .ToList()
                    );
                }
            }
            else
            {
                dgv_main.DataSource = null;
            }

            //Enable/disable navigation buttons
            btn_prev.Enabled = _currentIRIndex > 0;
            btn_next.Enabled = _currentIRIndex < _itemRequests.Count - 1;
        }

        private void ClearItemRequestUI()
        {
            _itemRequests = new List<ItemRequest2Model>();
            _currentIRIndex = -1;
            _previousIRIndex = -1;

            // Clear panel fields
            Helpers.ResetControls(_panels);

            // Clear grid
            dgv_main.DataSource = null;
            dgv_main.Rows.Clear();

            // Disable navigation buttons
            btn_prev.Enabled = false;
            btn_next.Enabled = false;
        }

        private async Task LoadUserList()
        {
            if (!_isEditing)
                return;

            try
            {
                userListServiceSetup = new GeneralService<UserListViewModel>(ApiEndPoints.ITEM_REQUEST2_USERS);
                _userdata = await userListServiceSetup.GetAsList();
            }
            catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "No Users found.");
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
        }

        private async Task LoadSalesOrderDoc()
        {
            try
            {
                salesOrderDocServiceSetup = new GeneralService<ItemRequestSalesOrderDocView>(ApiEndPoints.ITEM_REQUEST2_SO_DOC);
                _salesDocdata = await salesOrderDocServiceSetup.GetAsList();
            }
            catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "No Sales Order Doc found.");
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
        }

        private async Task LoadSalesOrders()
        {
            if (!_isEditing)
                return;

            if (_sales_order_id <= 0)
            {
                Helpers.ShowDialogMessage("error", "Please select a reference doc first.");
                return;
            }

            try
            {
                salesOrderServiceSetup = new GeneralService<ItemRequestSalesOrderView>(ApiEndPoints.ITEM_REQUEST2_SO + _sales_order_id);
                _salesdata = await salesOrderServiceSetup.GetAsList();

                // Bind sales_order_details_view to the datagridview
                if (_salesdata != null)
                {
                    // Get the sales_order_id from the first row since it's always the same
                    txt_sales_order_id.Text = _salesdata.FirstOrDefault()?.sales_order_id.ToString() ?? string.Empty;

                    _currentDetails = new BindingList<ItemRequestDetails2Model>(
                        _salesdata
                            .Select(d => new ItemRequestDetails2Model
                            {
                                sales_order_details_id = d.sales_order_details_id,
                                item_description = d.item_desc,
                                item_id = d.item_id,
                                required_qty = d.required_qty,
                                required_uom = d.required_uom,
                                remaining_qty = d.remaining_qty,
                                remaining_uom = d.remaining_uom
                            })
                            .ToList()
                    );

                    dgv_main.AutoGenerateColumns = false;
                    dgv_main.DataSource = _currentDetails;
                }
                else
                {
                    _currentDetails = new BindingList<ItemRequestDetails2Model>();
                    dgv_main.DataSource = _currentDetails;
                }
            }
            catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "No Sales Order found.");
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
        }

        private async void cmb_ref_doc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isEditing)
                return;

            if (cmb_ref_doc.SelectedItem is ItemRequestSalesOrderDocView selectedPurchaseOrder)
            {
                _sales_order_id = selectedPurchaseOrder.sales_order_id;
                txt_ref_doc.Text = selectedPurchaseOrder.so_doc_no;
                await LoadSalesOrders();
            }
            else
            {
                _sales_order_id = 0;
                _salesdata = null;
            }
        }

        private void cmb_received_by_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isEditing)
                return;
        }

        private void dgv_main_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!_isEditing)
                return;

            if (e.RowIndex < 0)
                return;

            // Open BinLocationPickComponent when warehouse user clicks issued_qty
            if (_isWarehouse && dgv_main.Columns[e.ColumnIndex].Name == "issued_qty")
            {
                var row = dgv_main.Rows[e.RowIndex];

                //checks the has_issued boolean flag instead
                var hasIssuedValue = row.Cells["has_issued"].Value;
                if (hasIssuedValue != null && hasIssuedValue != DBNull.Value
                    && bool.TryParse(hasIssuedValue.ToString(), out bool hasIssued)
                    && hasIssued)
                {
                    return;
                }

                var itemIdValue = row.Cells["item_id"].Value;

                if (itemIdValue == null || itemIdValue == DBNull.Value || !int.TryParse(itemIdValue.ToString(), out int itemId) || itemId <= 0)
                {
                    Helpers.ShowDialogMessage("error", "No item selected for this row.");
                    return;
                }

                using (var binLocationForm = new BinLocationPickComponent(itemId))
                {
                    if (binLocationForm.ShowDialog(this) == DialogResult.OK)
                    {
                        // Set the summed selected_qty into issued_qty of the clicked row
                        row.Cells["issued_qty"].Value = binLocationForm.TotalSelectedQty;

                        if (e.RowIndex < _currentDetails.Count)
                            _currentDetails[e.RowIndex].issued_qty = binLocationForm.TotalSelectedQty;

                        // 2. Resolve the detail ID for the clicked row
                        int detailId = e.RowIndex < _currentDetails.Count
                            ? _currentDetails[e.RowIndex].id
                            : 0;

                        int parentId = _currentIRIndex >= 0 && _itemRequests != null
                            ? _itemRequests[_currentIRIndex].id
                            : 0;

                        // 3. Remove any prior location selections for this detail line
                        //    (user may re-pick from the same row)
                        var pruned = _currentLocations
                            .Where(l => l.item_request_details_id != detailId)
                            .ToList();

                        // 4. Append the new selections
                        foreach (var sel in binLocationForm.SelectedLocations)
                        {
                            pruned.Add(new ItemRequestLocation2Model
                            {
                                item_request_id = parentId,
                                item_request_details_id = detailId,
                                bin_id = sel.BinId,
                                selected_qty = sel.SelectedQty
                            });
                        }

                        _currentLocations = new BindingList<ItemRequestLocation2Model>(pruned);
                    }
                }

                return;
            }

            if (dgv_main.Columns[e.ColumnIndex].Name == "item_description")
            {
                if(_isWarehouse) return;

                var row = dgv_main.Rows[e.RowIndex];

                // If row came from ref_doc (has a sales_order_details_id), block the picker
                var salesOrderDetailsId = row.Cells["sales_order_details_id"].Value;
                bool isFromRefDoc = salesOrderDetailsId != null
                    && salesOrderDetailsId != DBNull.Value
                    && !string.IsNullOrEmpty(salesOrderDetailsId.ToString())
                    && salesOrderDetailsId.ToString() != "0";

                if (isFromRefDoc)
                    return;

                using (var itemForm = new ItemRequestItems())
                {
                    if (itemForm.ShowDialog(this) == DialogResult.OK)
                    {
                        // Get or create the bound model for this row
                        ItemRequestDetails2Model detailModel;

                        if (e.RowIndex < _currentDetails.Count)
                        {
                            // Update existing bound entry
                            detailModel = _currentDetails[e.RowIndex];
                        }
                        else
                        {
                            // New row — create and add a new model entry
                            detailModel = new ItemRequestDetails2Model();
                            _currentDetails.Add(detailModel);
                        }

                        // Map item picker result into the model (mirrors LoadSalesOrders mapping)
                        detailModel.item_id = int.TryParse(itemForm.SelectedItemId, out int parsedId) ? parsedId : 0;
                        detailModel.item_description = itemForm.SelectedItemDesc;
                        detailModel.required_uom = itemForm.SelectedItemUom;

                        // Refresh the grid to reflect model changes
                        dgv_main.Refresh();
                    }
                }
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            if (!_isEditing)
                return;

            btn_cancel.Visible = false;
            btn_forward.Visible = true;
            _isForward = false;
        }

        private void btn_forward_Click(object sender, EventArgs e)
        {
            if (!_isEditing)
                return;

            btn_forward.Visible = false;
            btn_cancel.Visible = true;
            _isForward = true;
        }

        private void dgv_main_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Prevent default crash dialog
            e.ThrowException = false;

            Helpers.ShowDialogMessage("error", "Invalid numeric value. Please enter a valid amount.");
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

        private void dgv_main_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!_isEditing) return;
            if (e.RowIndex < 0) return;

            if (dgv_main.Columns[e.ColumnIndex].Name == "issued_qty")
            {
                var row = dgv_main.Rows[e.RowIndex];

                if (dgv_main.Columns.Contains("required_uom") && dgv_main.Columns.Contains("issued_uom"))
                {
                    var issuedQtyVal = row.Cells["issued_qty"].Value;
                    bool hasIssuedQty = issuedQtyVal != null
                        && issuedQtyVal != DBNull.Value
                        && decimal.TryParse(issuedQtyVal.ToString(), out decimal iq)
                        && iq > 0;

                    var newUom = hasIssuedQty ? row.Cells["required_uom"].Value : null;
                    row.Cells["issued_uom"].Value = newUom;

                    // Also update the bound model if it exists
                    if (e.RowIndex < _currentDetails.Count)
                    {
                        _currentDetails[e.RowIndex].issued_uom = newUom?.ToString();
                    }
                }
            }
        }

        private void dgv_main_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            Helpers.HandleNumericColumns(dgv_main, e, new[] { "required_qty", "issued_qty" });
        }
    }
}
