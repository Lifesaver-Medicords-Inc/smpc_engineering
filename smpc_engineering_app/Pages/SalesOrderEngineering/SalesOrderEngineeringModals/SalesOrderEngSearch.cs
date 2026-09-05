using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Models;
using smpc_engineering_app.Services;
using smpc_engineering_app.Services.Helpers;
using smpc_engineering_app.Shared;

namespace smpc_engineering_app.Pages.SalesOrderEngineering.SalesOrderEngineeringModals
{
    public partial class SalesOrderEngSearch : Form
    {
        public string SelectedSOId { get; private set; } = null;
        private string placeHolderText = "Sales Order Search...";
        private SalesOrderViewEngList SalesOrder;
        GeneralService<SalesOrderViewEngList> generalSalesOrder;
        private DataTable soTable;

        public SalesOrderEngSearch()
        {
            InitializeComponent();

            // Center the modal relative to its parent form
            this.StartPosition = FormStartPosition.CenterParent;

            dgv_so_search.AutoGenerateColumns = false;
            Helpers.DataGridViewDocumentFormatter.DataGridViewDocumentFormat(dgv_so_search, "doc_no", "SO#");
            InitializeSearchBox();
        }

        private void InitializeSearchBox()
        {
            txt_search = Helpers.CreateSearchBox(placeHolderText, txt_search_TextChanged);
            this.Controls.Add(txt_search);
        }

        private void txt_search_TextChanged(object sender, EventArgs e)
        {
            if (soTable == null || soTable.Rows.Count == 0)
                return;

            string searchText = txt_search.Text.Trim();

            if (string.IsNullOrEmpty(searchText) || searchText == placeHolderText)
            {
                dgv_so_search.DataSource = soTable;
            }
            else
            {
                var searchedData = Helpers.FilterDataTable(soTable, searchText,
                    "customer", "code", "tin", "delivery_date", "doc_no", "date", "delivery_to", "bill_to");
                dgv_so_search.DataSource = searchedData;
            }
        }

        private async void SalesOrderEngSearch_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_so_search, "Fetching data...");
                await SalesOrders();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_so_search);
            }
        }

        private async Task SalesOrders()
        {
            try
            {
                generalSalesOrder = new GeneralService<SalesOrderViewEngList>(ApiEndPoints.SALES_ORDER_ENGINEER);

                SalesOrder = await generalSalesOrder.GetAsModel();

                // Convert ap voucher list to DataTable using helper
                soTable = Helpers.ToDataTable(SalesOrder.sales_order_view);

                if (soTable?.Rows.Count > 0)
                {
                    dgv_so_search.DataSource = soTable;
                }
                else
                {
                    dgv_so_search.DataSource = null;
                    Helpers.ShowDialogMessage("error", "No sales order found.");
                }
            }
            catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "No sales order found.");
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
        }

        private void dgv_so_search_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var row = dgv_so_search.Rows[e.RowIndex];

            // Always get the id value from the row, regardless of which column was clicked
            var idValue = row.Cells["id"].Value;

            if (idValue != null)
            {
                SelectedSOId = idValue.ToString();

                this.DialogResult = DialogResult.OK; // close the modal with OK
                this.Close();
            }
        }
    }
}
