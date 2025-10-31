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
using smpc_engineering_app.Services.Setup;

namespace smpc_engineering_app.Pages.ItemRequest.ItemRequestModals
{
    public partial class ItemRequestItems : Form
    {
        public string SelectedItemId { get; private set; } = null;
        public string SelectedItemDesc { get; private set; } = null;
        public string SelectedItemUom { get; private set; } = null;
        private string placeHolderText = "Item List Search...";
        readonly ItemListService itemlistService = new ItemListService();
        private DataTable itemTable;

        public ItemRequestItems()
        {
            InitializeComponent();

            // Center the modal relative to its parent form
            this.StartPosition = FormStartPosition.CenterParent;

            dgv_all_item.AutoGenerateColumns = false;
            InitializeSearchBox();
        }

        private void InitializeSearchBox()
        {
            txt_search = Helpers.CreateSearchBox(placeHolderText, txt_search_TextChanged);
            this.Controls.Add(txt_search);
        }

        private void txt_search_TextChanged(object sender, EventArgs e)
        {
            if (itemTable == null || itemTable.Rows.Count == 0)
                return;

            string searchText = txt_search.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                dgv_all_item.DataSource = itemTable;
                return;
            }

            var searchedData = Helpers.FilterDataTable(itemTable, searchText, "short_desc", "item_code", "general_name", "item_model", "uom_name", "size");
            dgv_all_item.DataSource = searchedData;
        }

        private async void ItemRequestItems_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_all_item, "Fetching data...");
                await LoadItemLists();
            }
            catch(Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_all_item);
            }
        }

        private async Task LoadItemLists()
        {
            var data = await itemlistService.GetAsDatatable();
            itemTable = data;

            if (itemTable?.Rows.Count > 0)
            {
                dgv_all_item.DataSource = itemTable;
            }
            else
            {
                dgv_all_item.DataSource = null;
                Helpers.ShowDialogMessage("info", "No items found.");
            }
        }

        private void dgv_all_item_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var row = dgv_all_item.Rows[e.RowIndex];

            // Always get the id value from the row, regardless of which column was clicked
            var idValue = row.Cells["item_id"].Value;
            var descValue = row.Cells["short_desc"].Value;
            var uomvalue = row.Cells["uom_name"].Value;

            if (idValue != null)
            {
                SelectedItemId = idValue.ToString();
                SelectedItemDesc = descValue.ToString();
                SelectedItemUom = uomvalue.ToString();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
