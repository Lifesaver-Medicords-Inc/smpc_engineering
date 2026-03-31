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
using smpc_engineering_app.Services.Transaction;
using smpc_engineering_app.Shared;

namespace smpc_engineering_app.Pages.ItemRequest.ItemRequestModals
{
    public partial class ItemRequestSearch : Form
    {
        public string SelectedIRId { get; private set; } = null;
        private string placeHolderText = "Item Request Search...";
        private ItemRequestList ItemRequest;
        readonly ItemRequestService itemRequestService = new ItemRequestService();
        private DataTable irTable;
        private bool _isWarehouseUser;
        private string userDepartment = CacheData.CurrentUser.department.ToLower();

        public ItemRequestSearch()
        {
            InitializeComponent();

            // Center the modal relative to its parent form
            this.StartPosition = FormStartPosition.CenterParent;

            _isWarehouseUser = userDepartment == "warehouse";

            dgv_ir_search.AutoGenerateColumns = false;
            InitializeSearchBox();
        }

        private void InitializeSearchBox()
        {
            txt_search = Helpers.CreateSearchBox(placeHolderText, txt_search_TextChanged);
            this.Controls.Add(txt_search);
        }

        private void txt_search_TextChanged(object sender, EventArgs e)
        {
            if (irTable == null || irTable.Rows.Count == 0)
                return;

            string searchText = txt_search.Text.Trim();

            if (string.IsNullOrEmpty(searchText) || searchText == placeHolderText)
            {
                dgv_ir_search.DataSource = irTable;
            }
            else
            {
                var searchedData = Helpers.FilterDataTable(irTable, searchText, "doc_no", "ref_doc", "req_by", "req_date", "required_date");
                dgv_ir_search.DataSource = searchedData;
            }
        }

        private async void ItemRequestSearch_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_ir_search, "Fetching data...");
                await LoadItemRequests();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_ir_search);

            }
        }

        private async Task LoadItemRequests()
        {
            try
            {
                ItemRequest = await itemRequestService.GetAsModel();
                ItemRequest.item_request.Reverse();

                if (_isWarehouseUser)
                {
                    ItemRequest.item_request = ItemRequest.item_request
                        .Where(r => r.is_forward == true)
                        .ToList();
                }

                // Convert receiving report list to DataTable using helper
                irTable = Helpers.ToDataTable(ItemRequest.item_request);

                if (irTable?.Rows.Count > 0)
                {
                    dgv_ir_search.DataSource = irTable;
                }
                else
                {
                    dgv_ir_search.DataSource = null;
                    Helpers.ShowDialogMessage("error", "No items request found.");
                }
            }
            catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "No item request found.");
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
        }

        private void dgv_ir_search_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var row = dgv_ir_search.Rows[e.RowIndex];

            // Always get the id value from the row, regardless of which column was clicked
            var idValue = row.Cells["id"].Value;

            if (idValue != null)
            {
                SelectedIRId = idValue.ToString();

                this.DialogResult = DialogResult.OK; // close the modal with OK
                this.Close();
            }
        }
    }
}
