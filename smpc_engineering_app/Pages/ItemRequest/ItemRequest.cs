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
using smpc_engineering_app.Services.Transaction;
using smpc_engineering_app.Shared;

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

        private string userDepartment = CacheData.CurrentUser.department.ToLower();
        private readonly Panel[] _panels;
        private bool _isNewMode = false;
        private bool _isEditMode = false;
        private int _currentIRIndex = -1;
        private int _previousIRIndex = -1;
        private List<ItemRequestModel> _itemRequests;
        private ItemRequestList _irdata;

        public ItemRequest()
        {
            InitializeComponent();

            _panels = new[] { pnl_top, pnl_bot };
            Helpers.EnableGroupHeaders(dgv_main, columnGroupsMain);
            Helpers.SetChildControlsEnabled(_panels, false, new string[] { });
        }

        private void SetEditableColumns(bool isEdit)
        {
            var editableColumns = userDepartment != "warehouse"
                ? new[] { "remarks", "item_description", "req_qty" }
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
            dgv_main.AllowUserToAddRows = enable;
            _isNewMode = isNewMode;
            _isEditMode = !isNewMode && enable;

            var excludeControls = userDepartment != "warehouse"
                ? new[] { "txt_id", "txt_approved_by", "txt_issued_by", "txt_req_by", "txt_req_date", "txt_doc_no", "txt_received_by" }
                : new[] { "txt_required_date", "txt_id", "txt_approved_by", "txt_issued_by", "txt_req_by", "txt_req_date", "txt_doc_no" };

            Helpers.SetChildControlsEnabled(_panels, enable, excludeControls);

            Helpers.SetButtonVisibility(
                toolStrip1,
                visibleButtons: enable ? new[] { "btn_save", "btn_close" } : new[] { "btn_prev", "btn_next", "btn_search", "btn_new", "btn_edit", "btn_delete" },
                hiddenButtons: enable ? new[] { "btn_prev", "btn_next", "btn_search", "btn_new", "btn_edit", "btn_delete" } : new[] { "btn_save", "btn_close" }
            );
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

        private void btn_new_Click(object sender, EventArgs e)
        {
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
            SetEditMode(true);
        }

        private void btn_search_Click(object sender, EventArgs e)
        {
            var searchForm = new ItemRequestSearch();

            searchForm.ShowDialog();
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
                ShowCurrentRecord(); // Rebinds original data from _irdata
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {

        }

        private async void ItemRequest_Load(object sender, EventArgs e)
        {
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

            if (_irdata != null && _irdata.item_request != null && _irdata.item_request.Count > 0)
            {
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
            DataTable rrTable = Helpers.ToDataTable(_irdata.item_request);

            if (rrTable.Rows.Count == 0 || _currentIRIndex >= rrTable.Rows.Count)
                return;

            //Bind controls automatically (textboxes, checkboxes, etc.)
            Helpers.BindControls(_panels, rrTable, _currentIRIndex);

            var current = _itemRequests[_currentIRIndex];

            //Check if the current item request is forwarded
            if (current.is_forward == true)
            {
                btn_cancel.Visible = true;
                btn_forward.Visible = false;
            }
            else
            {
                btn_cancel.Visible = false;
                btn_forward.Visible = true;
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

        private void btn_forward_Click(object sender, EventArgs e)
        {

        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {

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
    }
}
