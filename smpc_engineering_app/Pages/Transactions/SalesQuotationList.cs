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

namespace smpc_engineering_app.Pages.Transactions
{
    // §3.2/§6.3 - "an engineer sees the quotations sent to them and no others" (the
    // REQUEST FOR ENGR. grant, Phase 4 item 4.1). This was previously a 22-line shell
    // with a PENDING/ONGOING tab split that had no real status to back it - a
    // quotation sent here carries no "engineer is done" signal anywhere in the
    // spec or data - so this is one flat list, scoped server-side to
    // CacheData.CurrentUser.id via GetEngineeringQuotationListByEngr's :userId route.
    public partial class SalesQuotationList : UserControl
    {
        private DataTable _quotationTable;

        public SalesQuotationList()
        {
            InitializeComponent();
            Helpers.Placeholder.SetPlaceholder(txt_search, "SEARCH BAR");

            this.Load += SalesQuotationList_Load;
            txt_search.TextChanged += txt_search_TextChanged;
            dgv_quotation_list.CellDoubleClick += dgv_quotation_list_CellDoubleClick;
        }

        private async void SalesQuotationList_Load(object sender, EventArgs e)
        {
            await LoadQuotationList();
        }

        private async Task LoadQuotationList()
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_quotation_list, "Fetching data...");

                int engrId = CacheData.CurrentUser?.id ?? 0;
                if (engrId <= 0)
                {
                    _quotationTable = null;
                    dgv_quotation_list.DataSource = null;
                    return;
                }

                var service = new GeneralService<RedboxQuotationList>(ApiEndPoints.SALES_QUOTATION_LIST + "/" + engrId);
                var response = await service.GetAsModel();
                var list = response?.quotationlist ?? new List<QuotationRedboxListModel>();

                _quotationTable = Helpers.ToDataTable(list);
                dgv_quotation_list.DataSource = _quotationTable;
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load Sales Quotation List: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_quotation_list);
            }
        }

        private void txt_search_TextChanged(object sender, EventArgs e)
        {
            if (_quotationTable == null || _quotationTable.Rows.Count == 0)
                return;

            string searchText = txt_search.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                dgv_quotation_list.DataSource = _quotationTable;
                return;
            }

            var searchedData = Helpers.FilterDataTable(_quotationTable, searchText,
                "client_name", "sales_quotation", "project_name",
                "sales_executive", "status", "remark");
            dgv_quotation_list.DataSource = searchedData;
        }

        // Opening the actual quotation (Size Up / item table / wiring editable, everything
        // else read-only per §3.2) is the next build step - the detail editor host page
        // doesn't exist yet, so this only reports that for now rather than silently
        // doing nothing on a double-click.
        private void dgv_quotation_list_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgv_quotation_list.Rows[e.RowIndex];
            int quotationId = Convert.ToInt32(row.Cells["col_id"].Value ?? 0);
            if (quotationId <= 0) return;

            Helpers.ShowDialogMessage("info", "The quotation editor for Engineering isn't wired up yet.");
        }
    }
}
