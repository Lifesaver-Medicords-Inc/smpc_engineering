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
using smpc_engineering_app.Pages.PickActivity.PickActivityModals;
using smpc_engineering_app.Pages.Components;

namespace smpc_engineering_app.Pages.PickActivity2
{
    public partial class PickActivityPage2 : UserControl
    {
        PickActivityService2 pickActivityService = new PickActivityService2();
        private int _currentPAIndex = -1;
        private int _previousPAIndex = -1;
        private PickActivityList2 _padata;
        private List<PickActivity2Model> _pickActivities;
        private List<PickActivitySalesOrderDocView> _salesDocdata;
        private SalesOrderViewList _salesdata;
        private List<PickActWarehouseViewModel> _warehousedata;
        private Dictionary<int, List<PickActWarehouseAreaView>> _warehouseAreaCache = new Dictionary<int, List<PickActWarehouseAreaView>>();
        private DataTable _paTable;
        private BindingList<PickActivityDetails2Model> _currentDetails;
        private bool _isNewMode = false;
        private bool _isEditMode = false;
        private bool _isEditing = false;
        GeneralService<PickActWarehouseViewModel> warehouseServiceSetup;
        GeneralService<PickActWarehouseAreaView> warehouseAreaServiceSetup;
        GeneralService<PickActivitySalesOrderDocView> salesOrderDocServiceSetup;
        GeneralService<SalesOrderViewList> salesOrderServiceSetup;
        private int _sales_order_id;
        private int _warehouse_id;
        private string _userDepartment = CacheData.CurrentUser.department.ToLower();
        private string _userName = CacheData.CurrentUser.first_name + " " + CacheData.CurrentUser.last_name;
        private bool _isWarehouse;
        private BinLocationComboOverlay _binLocationOverlay;
        private BindingList<PickActivityLocation2Model> _currentLocations = new BindingList<PickActivityLocation2Model>();

        //Dictionaries for the column grouping of datagridviews
        Dictionary<string, string[]> columnGroupsMain = new Dictionary<string, string[]>()
        {
            { "QTY LEFT", new string[] { "left_qty", "left_uom" } },
            { "QTY TO PICK", new string[] { "pick_qty", "pick_uom"} },
            { "ACTUAL PICK QTY", new string[] { "actual_qty", "actual_uom" } },
        };

        public PickActivityPage2()
        {
            InitializeComponent();
            Helpers.EnableGroupHeaders(dgv_main, columnGroupsMain);
            _userName = CacheData.CurrentUser.first_name + " " + CacheData.CurrentUser.last_name;

            _binLocationOverlay = new BinLocationComboOverlay(
                dgv_main,
                rowIndex =>
                {
                    if (rowIndex < 0 || rowIndex >= dgv_main.Rows.Count)
                        return new List<PickActWarehouseAreaView>();

                    var cellVal = dgv_main.Rows[rowIndex].Cells["cmb_warehouse"].Value;
                    if (cellVal == null || cellVal == DBNull.Value
                        || !int.TryParse(cellVal.ToString(), out int wId) || wId <= 0)
                        return new List<PickActWarehouseAreaView>();

                    _warehouseAreaCache.TryGetValue(wId, out var areaList);
                    return areaList ?? new List<PickActWarehouseAreaView>();
                }
            );
        }

        private void SetEditableColumns(bool isEdit)
        {
            var alwaysEditableColumns = _isWarehouse ? Array.Empty<string>() : new[] { "pick_qty" };
            var newModeOnlyColumns = new[] { "pick_qty" };

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
            ToggleWarehouseColumns(isEdit);
        }

        private void ApplyWarehouseRestrictions(bool isEditMode = false)
        {
            if (!_isWarehouse) return;

            pnl_top.Enabled = true;

            // Hide new and delete buttons
            if (toolStrip1.Items.ContainsKey("btn_new"))
                toolStrip1.Items["btn_new"].Visible = false;
            if (toolStrip1.Items.ContainsKey("btn_delete"))
                toolStrip1.Items["btn_delete"].Visible = false;

            // Keep serial_no, remarks editable in edit mode
            string[] warehouseEditableColumns = { "bin_location", "cmb_warehouse" };
            foreach (var colName in warehouseEditableColumns)
            {
                if (dgv_main.Columns.Contains(colName))
                {
                    var column = dgv_main.Columns[colName];
                    column.ReadOnly = !isEditMode;
                    column.DefaultCellStyle.BackColor = isEditMode ? Color.White : Color.Gainsboro;
                }

                var warehouseColumn =
                    (DataGridViewComboBoxColumn)dgv_main.Columns["cmb_warehouse"];

                warehouseColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
                warehouseColumn.FlatStyle = FlatStyle.Flat;

                // THIS allows null values
                warehouseColumn.DefaultCellStyle.NullValue = "";
            }
        }

        private void ToggleWarehouseColumns(bool isEditing)
        {
            if (dgv_main.Columns.Contains("cmb_warehouse"))
                dgv_main.Columns["cmb_warehouse"].Visible = isEditing;

            if (dgv_main.Columns.Contains("warehouse"))
                dgv_main.Columns["warehouse"].Visible = !isEditing;
        }

        private async Task SetEditMode(bool enable, bool isNewMode = false)
        {
            _isNewMode = isNewMode;
            _isEditing = enable;
            _isEditMode = enable && !isNewMode;

            SetEditableColumns(enable);

            // buttons
            string[] editButtons = { "btn_save", "btn_close", "btn_forward", "btn_cancel" };
            string[] navButtons = { "btn_new", "btn_print", "btn_edit", "btn_delete", "btn_next", "btn_prev", "btn_search" };

            Helpers.SetButtonVisibility2(
                toolStrip1,
                pnl_top,
                visibleButtons: enable ? editButtons : navButtons,
                hiddenButtons: enable ? navButtons : editButtons
            );

            Helpers.SetChildControlsEnabled(new Panel[] { pnl_top }, !enable, new string[] { "txt_doc_no", "txt_picked_by",
                "txt_prepared_by", "txt_customer", "txt_customer_code", "txt_sales_person" });

            // Manually control cmb_ref_doc: enabled only in new mode, disabled in edit mode
            cmb_reference_so.Enabled = enable && _isNewMode;
            cmb_reference_so.DropDownStyle = (enable && _isNewMode) ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown;
            cmb_reference_so.BackColor = (enable && _isNewMode) ? Color.White : Color.FromArgb(235, 235, 235);

            // Load and bind combos only when entering edit or new mode
            if (enable)
            {
                await LoadSalesOrderDoc();
                await LoadWarehouse();

                // ── Bind warehouse combobox column ──
                if (_warehousedata != null && dgv_main.Columns.Contains("cmb_warehouse"))
                {
                    var cmbCol = dgv_main.Columns["cmb_warehouse"] as DataGridViewComboBoxColumn;
                    if (cmbCol != null)
                    {
                        // Unwire before binding
                        dgv_main.CellValueChanged -= dgv_main_CellValueChanged;
                        dgv_main.CurrentCellDirtyStateChanged -= dgv_main_CurrentCellDirtyStateChanged;

                        cmbCol.DataSource = _warehousedata;
                        cmbCol.DisplayMember = "warehouse";
                        cmbCol.ValueMember = "warehouse_id";

                        SyncWarehouseComboFromData();

                        // Rewire after binding
                        dgv_main.CellValueChanged += dgv_main_CellValueChanged;
                        dgv_main.CurrentCellDirtyStateChanged += dgv_main_CurrentCellDirtyStateChanged;
                    }
                }

                cmb_reference_so.SelectedIndexChanged -= cmb_reference_so_SelectedIndexChanged;

                if (_salesDocdata != null)
                {
                    cmb_reference_so.DataSource = _salesDocdata;
                    cmb_reference_so.DisplayMember = "so_doc_no";
                    cmb_reference_so.ValueMember = "sales_order_id";

                    // §2.5: a Sales Order reads SO#0001 everywhere it is shown. Display only -
                    // SelectedItem/SelectedValue are untouched.
                    Helpers.ComboBoxDocumentFormatter.ComboBoxDocumentFormat(cmb_reference_so, "SO#");
                }

                if (_isEditMode)
                {
                    cmb_reference_so.SelectedIndex = -1;

                    // txt_reference_so holds the raw stored value - prefix it so the closed
                    // box and the open dropdown agree.
                    cmb_reference_so.Text = Helpers.ComboBoxDocumentFormatter.FormatDocumentNo("SO#", txt_reference_so.Text);
                }

                cmb_reference_so.SelectedIndexChanged += cmb_reference_so_SelectedIndexChanged;
            }
            else
            {
                // Clear warehouse column on exit
                if (dgv_main.Columns.Contains("cmb_warehouse"))
                {
                    var cmbCol = dgv_main.Columns["cmb_warehouse"] as DataGridViewComboBoxColumn;
                    if (cmbCol != null)
                    {
                        dgv_main.CellValueChanged -= dgv_main_CellValueChanged;
                        dgv_main.CurrentCellDirtyStateChanged -= dgv_main_CurrentCellDirtyStateChanged;

                        cmbCol.DataSource = null;

                        dgv_main.CellValueChanged += dgv_main_CellValueChanged;
                        dgv_main.CurrentCellDirtyStateChanged += dgv_main_CurrentCellDirtyStateChanged;
                    }
                }


                // Clear combos when leaving edit mode
                cmb_reference_so.DataSource = null;
            }

            ApplyWarehouseRestrictions(enable);

            _binLocationOverlay.SetEditingMode(enable);
        }

        private void ChangeRecord(int step)
        {
            if (_pickActivities == null || !_pickActivities.Any()) return;

            int newIndex = _currentPAIndex + step;
            if (newIndex >= 0 && newIndex < _pickActivities.Count)
            {
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

        private async void btn_cancel_Click(object sender, EventArgs e)
        {
            await SetEditMode(false);

            // If no records exist, clear everything
            if (_pickActivities == null || !_pickActivities.Any())
            {
                ClearPickActivityUI();
                return;
            }

            // Return to the previous record index if available
            if (_previousPAIndex >= 0 && _pickActivities != null && _pickActivities.Count > 0)
            {
                _currentPAIndex = _previousPAIndex;
                await LoadPickActivities();
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

        private async void btn_new_Click(object sender, EventArgs e)
        {
            // Save current index before clearing
            _previousPAIndex = _currentPAIndex;
            await SetEditMode(true, isNewMode: true);

            //Clear only the rows, keep columns
            _currentDetails = new BindingList<PickActivityDetails2Model>();
            dgv_main.AutoGenerateColumns = false;
            dgv_main.DataSource = _currentDetails;
            Helpers.ResetControls(new Panel[] { pnl_top });
        }

        private async void btn_edit_Click(object sender, EventArgs e)
        {
            if (_currentPAIndex < 0 || _pickActivities == null || !_pickActivities.Any())
            {
                Helpers.ShowDialogMessage("error", "No record selected to edit.");
                return;
            }

            // Store last viewed record index
            _previousPAIndex = _currentPAIndex;

            await SetEditMode(true);
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

                var pickActivityParent = Helpers.BuildModelFromPanels<PickActivity2Model>(new Panel[] { pnl_top });
                var pickActivityDetails = Helpers.DatagridviewMapper.BuildModelsFromData<PickActivityDetails2Model>(dgv_main);
                var pickActivityLocations = _currentLocations?.ToList() ?? new List<PickActivityLocation2Model>();

                var paPayload = new PickActivityPayload2
                {
                    pick_activity = pickActivityParent,
                    pick_activity_details = pickActivityDetails,
                    pick_activity_locations = pickActivityLocations
                };

                var result = await pickActivityService.DeletePickActivity(paPayload);

                if (!result.success)
                {
                    Helpers.ShowDialogMessage("error", "Pick Activity not deleted.");
                    return;
                }

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
            btn_save.Enabled = false;
            btn_cancel.Enabled = false;

            _binLocationOverlay.Hide();

            try
            {
                dgv_main.EndEdit();

                if (Helpers.ValidateControlsValues(pnl_top))
                {
                    Helpers.ShowDialogMessage("error", "Please fill in all required fields.");
                    return;
                }

                //Validate Datagridview columns
                string[] columnsToValidate = { "item_description" };
                if (await Helpers.ValidateDataGridViewCells(dgv_main, columnsToValidate))
                    return;

                // Validate pick activity detail quantities
                if (!ValidatePickActDetails())
                    return;

                var pickActivityParent = Helpers.BuildModelFromPanels<PickActivity2Model>(new Panel[] { pnl_top });

                if (_isWarehouse)
                {
                    pickActivityParent.picked_by = _userName;
                }
                else
                {
                    pickActivityParent.prepared_by = _userName;
                }

                //Validate that pick activity details are not empty
                var pickActivityDetails = Helpers.DatagridviewMapper.BuildModelsFromData<PickActivityDetails2Model>(dgv_main)
                    .Where(d => d.has_actual != true)
                    .ToList();

                // ── Back-fill warehouse_id from the ComboBox column ──
                for (int i = 0; i < dgv_main.Rows.Count && i < pickActivityDetails.Count; i++)
                {
                    var cell = dgv_main.Rows[i].Cells["cmb_warehouse"].Value;
                    if (cell != null && cell != DBNull.Value
                        && int.TryParse(cell.ToString(), out int wId))
                    {
                        pickActivityDetails[i].warehouse_id = wId;
                    }
                }

                // Collect detail IDs that already have actual committed (has_actual == true on the row)
                var lockedDetailIds = new HashSet<int>(
                    _currentDetails
                        .Where(d => d.has_actual == true)
                        .Select(d => d.id)
                );

                var pickActivityLocations = _currentLocations?
                    .Where(l => !lockedDetailIds.Contains(l.pick_activity_details_id))
                    .ToList()
                    ?? new List<PickActivityLocation2Model>();

                // Set has_actual = true if actual_qty > 0
                foreach (var detail in pickActivityDetails)
                {
                    if (detail.actual_qty > 0)
                    {
                        detail.has_actual = true;
                    }
                    else
                    {
                        detail.has_actual = false;
                    }
                }

                if (pickActivityDetails == null || pickActivityDetails.Count == 0)
                {
                    Helpers.ShowDialogMessage("error", "Pick Activity cannot be empty.");
                    return;
                }

                // Wrap everything into Pick Activity Payload
                var paPayload = new PickActivityPayload2
                {
                    pick_activity = pickActivityParent,
                    pick_activity_details = pickActivityDetails,
                    pick_activity_locations = pickActivityLocations
                };

                Helpers.Loading.ShowLoading(dgv_main, "Saving data...");

                if (_isNewMode)
                {
                    var result = await pickActivityService.CreatePickActivity(paPayload);

                    if (!result.success)
                    {
                        Helpers.ShowDialogMessage("error", "Pick Activity not created.");
                        return;
                    }

                    Helpers.ShowDialogMessage("success", "Pick Activity created successfully.");
                }
                else
                {
                    var result = await pickActivityService.UpdatePickActivity(paPayload);

                    if (!result.success)
                    {
                        Helpers.ShowDialogMessage("error", "Pick Activity not updated.");
                        return;
                    }

                    Helpers.ShowDialogMessage("success", "Pick Activity updated successfully.");
                }

                await SetEditMode(false);
                await LoadPickActivities();
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

        private bool ValidatePickActDetails()
        {
            if (_isEditMode && !_isWarehouse)
                return true;

            bool hasAtLeastOnePick = false;
            bool hasAtLeastOneActual = false;

            for (int i = 0; i < dgv_main.Rows.Count; i++)
            {
                var row = dgv_main.Rows[i];

                // Get cell values
                var pickQtyVal = row.Cells["pick_qty"].Value;
                var actualQtyVal = row.Cells["actual_qty"].Value;
                var leftQtyVal = row.Cells["left_qty"].Value;
                var binLocVal = row.Cells["bin_location"].Value;

                decimal pickQty = 0, actualQty = 0, leftQty = 0;

                decimal.TryParse(pickQtyVal?.ToString(), out pickQty);
                decimal.TryParse(actualQtyVal?.ToString(), out actualQty);
                decimal.TryParse(leftQtyVal?.ToString(), out leftQty);

                // Check if at least one row has pick qty
                if (pickQty > 0)
                    hasAtLeastOnePick = true;

                // Check if at least one row has actual qty
                if (actualQty > 0)
                    hasAtLeastOneActual = true;

                // Validate: required must not exceed remaining
                if (!_isWarehouse && !(_isEditMode && !_isWarehouse))
                {
                    if (pickQty > leftQty)
                    {
                        string itemDesc = row.Cells["item_description"].Value?.ToString() ?? $"Row {i + 1}";
                        Helpers.ShowDialogMessage("error",
                            $"Row {i + 1} ({itemDesc}): The pick ({pickQty}) quantity exceeds the left quantity ({leftQty}).");
                        return false;
                    }
                }

                // Validate: actual must not exceed pick
                if (actualQty > pickQty)
                {
                    string itemDesc = row.Cells["item_description"].Value?.ToString() ?? $"Row {i + 1}";
                    Helpers.ShowDialogMessage("error",
                        $"Row {i + 1} ({itemDesc}): The actual ({actualQty}) quantity exceeds the pick quantity ({pickQty}).");
                    return false;
                }

                // Validate: actual qty requires bin location
                if (actualQty > 0)
                {
                    bool hasBinLoc = binLocVal != null
                        && !string.IsNullOrWhiteSpace(binLocVal.ToString());

                    if (!hasBinLoc)
                    {
                        string itemDesc = row.Cells["item_description"].Value?.ToString() ?? $"Row {i + 1}";
                        Helpers.ShowDialogMessage("error",
                            $"Row {i + 1} ({itemDesc}): bin location is required when a actual quantity is entered.");
                        return false;
                    }
                }

                // Validate: if warehouse is selected, actual_qty and bin_location are required
                var warehouseVal = row.Cells["cmb_warehouse"].Value;
                bool hasWarehouse = warehouseVal != null
                    && warehouseVal != DBNull.Value
                    && int.TryParse(warehouseVal.ToString(), out int wId)
                    && wId > 0;

                if (hasWarehouse)
                {
                    if (actualQty <= 0)
                    {
                        string itemDesc = row.Cells["item_description"].Value?.ToString() ?? $"Row {i + 1}";
                        Helpers.ShowDialogMessage("error",
                            $"Row {i + 1} ({itemDesc}): actual quantity is required when a warehouse is selected.");
                        return false;
                    }

                    bool hasBinLoc = binLocVal != null
                        && !string.IsNullOrWhiteSpace(binLocVal.ToString());

                    if (!hasBinLoc)
                    {
                        string itemDesc = row.Cells["item_description"].Value?.ToString() ?? $"Row {i + 1}";
                        Helpers.ShowDialogMessage("error",
                            $"Row {i + 1} ({itemDesc}): bin location is required when a warehouse is selected.");
                        return false;
                    }
                }
            }

            // Validate: at least one row must have a pick qty
            if (!hasAtLeastOnePick)
            {
                Helpers.ShowDialogMessage("error", "At least one item must have a pick quantity greater than zero.");
                return false;
            }

            if (_isWarehouse)
            {
                // Validate: at least one row must have a actual qty
                if (!hasAtLeastOneActual)
                {
                    Helpers.ShowDialogMessage("error", "At least one item must have a actual quantity greater than zero.");
                    return false;
                }
            }
            
            return true;
        }

        private async void PickActivityPage2_Load(object sender, EventArgs e)
        {
            try
            {
                _isWarehouse = _userDepartment == "warehouse";

                Helpers.Loading.ShowLoading(dgv_main, "Fetching data...");
                await LoadPickActivities();

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

        private async Task LoadPickActivities()
        {
            // save current index before reload
            int oldIndex = _currentPAIndex;

            //fill this declared value by the Pick Activity data
            _padata = await pickActivityService.GetAsModel();

            if (_padata != null && _padata.pick_activity != null && _padata.pick_activity.Count > 0)
            {
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
                ClearPickActivityUI();
            }
        }

        private void ShowCurrentRecord()
        {
            if (_currentPAIndex < 0 || _padata == null || _padata.pick_activity == null || !_padata.pick_activity.Any())
                return;

            // Convert pick activity list to DataTable using helper
            _paTable = Helpers.ToDataTable(_padata.pick_activity);

            Helpers.BindControls(new Panel[] { pnl_top }, _paTable, _currentPAIndex);

            //Disable auto column generation before setting the data source
            dgv_main.AutoGenerateColumns = false;

            var current = _pickActivities[_currentPAIndex];

            //Bind child details (grids)
            if (_padata?.pick_activity_details != null)
            {
                _currentDetails = new BindingList<PickActivityDetails2Model>(
                    _padata.pick_activity_details
                        .Where(d => d.pick_activity_id == current.id)
                        .ToList()
                );

                dgv_main.DataSource = _currentDetails;

                //Bind child locations
                if (_padata?.pick_activity_locations != null)
                {
                    _currentLocations = new BindingList<PickActivityLocation2Model>(
                        _padata.pick_activity_locations
                            .Where(d => d.pick_activity_id == current.id)
                            .ToList()
                    );
                }
            }
            else
            {
                dgv_main.DataSource = null;
            }

            //Enable/disable navigation buttons
            btn_prev.Enabled = _currentPAIndex > 0;
            btn_next.Enabled = _currentPAIndex < _pickActivities.Count - 1;
        }

        private void ClearPickActivityUI()
        {
            _pickActivities = new List<PickActivity2Model>();
            _currentPAIndex = -1;
            _previousPAIndex = -1;

            // Clear panel fields
            Helpers.ResetControls(new Panel[] { pnl_top });

            // Clear grid
            dgv_main.DataSource = null;
            dgv_main.Rows.Clear();

            // Disable navigation buttons
            btn_prev.Enabled = false;
            btn_next.Enabled = false;
            ToggleWarehouseColumns(false);
        }

        private async Task LoadWarehouse()
        {
            if (!_isEditing)
                return;

            try
            {
                warehouseServiceSetup = new GeneralService<PickActWarehouseViewModel>(ApiEndPoints.PICK_ACTIVITY2_WAREHOUSE);
                _warehousedata = await warehouseServiceSetup.GetAsList();
            }
            catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "No Warehouse found.");
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
        }

        private async Task LoadWarehouseArea(int warehouseId)
        {
            if (!_isEditing) return;
            if (warehouseId <= 0) return;

            // Skip network call if already cached
            if (_warehouseAreaCache.ContainsKey(warehouseId)) return;

            try
            {
                warehouseAreaServiceSetup = new GeneralService<PickActWarehouseAreaView>(
                    ApiEndPoints.PICK_ACTIVITY2_WAREHOUSE_AREA + warehouseId);
                var result = await warehouseAreaServiceSetup.GetAsList();
                _warehouseAreaCache[warehouseId] = result ?? new List<PickActWarehouseAreaView>();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load warehouse area: {ex.Message}");
            }
        }

        private async Task LoadSalesOrderDoc()
        {
            try
            {
                salesOrderDocServiceSetup = new GeneralService<PickActivitySalesOrderDocView>(ApiEndPoints.ITEM_REQUEST2_SO_DOC);
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
                salesOrderServiceSetup = new GeneralService<SalesOrderViewList>(ApiEndPoints.PICK_ACTIVITY2_SO + _sales_order_id);
                _salesdata = await salesOrderServiceSetup.GetAsModel();

                // Bind sales_order_details_view to the datagridview
                if (_salesdata != null)
                {
                    var soTable = Helpers.ToDataTable(_salesdata.sales_order_view);
                    Helpers.BindControls(new Panel[] { pnl_top }, soTable, 0);

                    _currentDetails = new BindingList<PickActivityDetails2Model>(
                        _salesdata.sales_order_details_view
                            .Select(d => new PickActivityDetails2Model
                            {
                                sales_order_details_id = d.sales_order_details_id,
                                item_code = d.item_code,
                                item_description = d.item_description,
                                item_id = d.item_id,
                                left_qty = d.left_qty,
                                left_uom = d.left_uom
                            })
                            .ToList()
                    );

                    dgv_main.AutoGenerateColumns = false;
                    dgv_main.DataSource = _currentDetails;
                }
                else
                {
                    _currentDetails = new BindingList<PickActivityDetails2Model>();
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

        private async void cmb_reference_so_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isEditing)
                return;

            if (cmb_reference_so.SelectedItem is PickActivitySalesOrderDocView selectedSalesOrder)
            {
                _sales_order_id = selectedSalesOrder.sales_order_id;
                txt_reference_so.Text = selectedSalesOrder.so_doc_no;
                await LoadSalesOrders();
            }
            else
            {
                _sales_order_id = 0;
                _salesdata = null;
            }
        }

        private void dgv_main_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Immediately commit the ComboBox selection so CellValueChanged fires
            if (dgv_main.IsCurrentCellDirty &&
                dgv_main.CurrentCell?.OwningColumn.Name == "cmb_warehouse")
            {
                dgv_main.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private async void dgv_main_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!_isEditing)
                return;

            if (e.RowIndex < 0) return;

            var colName = dgv_main.Columns[e.ColumnIndex].Name;
            var row = dgv_main.Rows[e.RowIndex];

            if (colName == "pick_qty")
            {
                var pickQtyVal = row.Cells["pick_qty"].Value;
                bool hasPickQty = pickQtyVal != null
                    && pickQtyVal != DBNull.Value
                    && decimal.TryParse(pickQtyVal.ToString(), out decimal pq)
                    && pq > 0;

                if (dgv_main.Columns.Contains("left_uom") && dgv_main.Columns.Contains("pick_uom"))
                {
                    row.Cells["pick_uom"].Value = hasPickQty
                        ? row.Cells["left_uom"].Value
                        : null; // ← clear UOM when qty is cleared
                }

                return;
            }

            if (colName == "actual_qty")
            {
                var actualQtyVal = row.Cells["actual_qty"].Value;
                bool hasActualQty = actualQtyVal != null
                    && actualQtyVal != DBNull.Value
                    && decimal.TryParse(actualQtyVal.ToString(), out decimal aq)
                    && aq > 0;

                if (dgv_main.Columns.Contains("pick_uom") && dgv_main.Columns.Contains("actual_uom"))
                {
                    row.Cells["actual_uom"].Value = hasActualQty
                        ? row.Cells["pick_uom"].Value
                        : null; // ← clear UOM when qty is cleared
                }

                return;
            }

            if (dgv_main.Columns[e.ColumnIndex].Name != "cmb_warehouse") return;

            var selectedValue = dgv_main.Rows[e.RowIndex].Cells["cmb_warehouse"].Value;

            if (selectedValue == null || selectedValue == DBNull.Value) return;

            if (int.TryParse(selectedValue.ToString(), out int warehouseId))
            {
                _warehouse_id = warehouseId;

                var match = _warehousedata?.FirstOrDefault(w => w.warehouse_id == warehouseId);
                if (match != null && dgv_main.Columns.Contains("warehouse"))
                {
                    dgv_main.Rows[e.RowIndex].Cells["warehouse"].Value = match.warehouse;
                }

                await LoadWarehouseArea(warehouseId);
                _binLocationOverlay.RefreshAreaData();   // re-populate if this row is active
            }
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

        private void dgv_main_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!_isEditing)
                return;

            if (e.RowIndex < 0)
                return;

            // Open BinLocationPickComponent when warehouse user clicks actual_qty
            if (_isWarehouse && dgv_main.Columns[e.ColumnIndex].Name == "actual_qty")
            {
                var row = dgv_main.Rows[e.RowIndex];

                //checks the has_actual boolean flag instead
                var hasActualValue = row.Cells["has_actual"].Value;
                if (hasActualValue != null && hasActualValue != DBNull.Value
                    && bool.TryParse(hasActualValue.ToString(), out bool hasActual)
                    && hasActual)
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
                        // Set the summed selected_qty into actual_qty of the clicked row
                        row.Cells["actual_qty"].Value = binLocationForm.TotalSelectedQty;

                        if (e.RowIndex < _currentDetails.Count)
                            _currentDetails[e.RowIndex].actual_qty = binLocationForm.TotalSelectedQty;

                        // 2. Resolve the detail ID for the clicked row
                        int detailId = e.RowIndex < _currentDetails.Count
                            ? _currentDetails[e.RowIndex].id
                            : 0;

                        int parentId = _currentPAIndex >= 0 && _pickActivities != null
                            ? _pickActivities[_currentPAIndex].id
                            : 0;

                        // 3. Remove any prior location selections for this detail line
                        //    (user may re-pick from the same row)
                        var pruned = _currentLocations
                            .Where(l => l.pick_activity_details_id != detailId)
                            .ToList();

                        // 4. Append the new selections
                        foreach (var sel in binLocationForm.SelectedLocations)
                        {
                            pruned.Add(new PickActivityLocation2Model
                            {
                                pick_activity_id = parentId,
                                pick_activity_details_id = detailId,
                                bin_id = sel.BinId,
                                selected_qty = sel.SelectedQty
                            });
                        }

                        _currentLocations = new BindingList<PickActivityLocation2Model>(pruned);
                    }
                }

                return;
            }
        }

        private void dgv_main_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var colName = dgv_main.Columns[e.ColumnIndex].Name;
            if (colName != "bin_location" && colName != "cmb_warehouse") return;

            if (IsRowHasActual(e.RowIndex))
                e.Cancel = true;
        }

        private void dgv_main_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var colName = dgv_main.Columns[e.ColumnIndex].Name;
            if (colName != "bin_location" && colName != "cmb_warehouse") return;

            if (!IsRowHasActual(e.RowIndex)) return;

            // Grey background — visually consistent with other read-only cells
            e.CellStyle.BackColor = Color.Gainsboro;

            // For the ComboBox column: suppress the dropdown arrow
            if (colName == "cmb_warehouse" &&
                dgv_main.Rows[e.RowIndex].Cells[e.ColumnIndex]
                    is DataGridViewComboBoxCell cmbCell)
            {
                cmbCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            }
        }

        private bool IsRowHasActual(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgv_main.Rows.Count) return false;

            var cell = dgv_main.Rows[rowIndex].Cells["has_actual"];
            return cell?.Value != null
                && cell.Value != DBNull.Value
                && bool.TryParse(cell.Value.ToString(), out bool v)
                && v;
        }

        private void SyncWarehouseComboFromData()
        {
            if (_warehousedata == null) return;

            for (int i = 0; i < dgv_main.Rows.Count; i++)
            {
                var row = dgv_main.Rows[i];

                // Pull warehouse_id from the bound model
                var warehouseIdCell = row.Cells["warehouse_id"];
                if (warehouseIdCell?.Value == null
                    || warehouseIdCell.Value == DBNull.Value
                    || !int.TryParse(warehouseIdCell.Value.ToString(), out int wId)
                    || wId <= 0)
                    continue;

                // Only set if the warehouse_id exists in the loaded list
                var match = _warehousedata.FirstOrDefault(w => w.warehouse_id == wId);
                if (match == null) continue;

                row.Cells["cmb_warehouse"].Value = wId;
            }
        }
    }
}
