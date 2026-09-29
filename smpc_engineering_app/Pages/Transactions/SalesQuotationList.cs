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

            // Show the designer's columns and nothing else. Every one of them names its
            // DataPropertyName, so auto-generation adds only what the designer chose NOT to
            // show - requested_engr_id and requested_engr_name turned up as two raw columns
            // at the end of the grid (user-reported 2026-09-24). Neither is populated any
            // more (REQUEST FOR ENGR. no longer names an engineer), and anything the API adds
            // to this list later stays off screen until someone gives it a column on purpose.
            dgv_quotation_list.AutoGenerateColumns = false;

            this.Load += SalesQuotationList_Load;
            txt_search.TextChanged += txt_search_TextChanged;
            dgv_quotation_list.CellDoubleClick += dgv_quotation_list_CellDoubleClick;

            QuotationListPushed += OnQuotationListPushed;
            this.Disposed += (s, e) => QuotationListPushed -= OnQuotationListPushed;
        }

        // ---- Live list ------------------------------------------------------------
        //
        // This page used to read the list once, on open, and never again - so a quotation
        // Sales requested while an engineer had the list open simply never appeared until
        // they closed the page and came back (user-reported 2026-09-24: "it will always
        // appear when updating on the end of the sales app").
        //
        // The engineering app already holds a socket that receives exactly this list - the
        // red box's (/ws/engineering/redboxlist/quotation, same view, same shape) - and
        // the API now sends on it after every project quotation save. Layout owns that
        // socket and republishes what arrives here, so this page needs no connection of
        // its own. Static because Layout has no reference to however many copies of this
        // page happen to be open; each subscribes for as long as it exists.
        internal static event Action<RedboxQuotationList> QuotationListPushed;

        internal static void PublishPushedList(RedboxQuotationList data)
        {
            QuotationListPushed?.Invoke(data);
        }

        // Arrives on the UI thread (Layout marshals it). Rebinds in place: no loading
        // overlay, and the engineer keeps their search text, their selected row and their
        // scroll position - a list updating under you must not also move you.
        private void OnQuotationListPushed(RedboxQuotationList data)
        {
            if (IsDisposed || data == null) return;

            int? selectedId = null;
            if (dgv_quotation_list.CurrentRow != null && dgv_quotation_list.Columns.Contains("col_id"))
            {
                object v = dgv_quotation_list.CurrentRow.Cells["col_id"].Value;
                if (v != null && int.TryParse(v.ToString(), out int id)) selectedId = id;
            }

            int firstRow = dgv_quotation_list.FirstDisplayedScrollingRowIndex;

            _quotationTable = Helpers.ToDataTable(data.quotationlist ?? new List<QuotationRedboxListModel>());
            ApplySearch();

            if (selectedId.HasValue)
            {
                foreach (DataGridViewRow row in dgv_quotation_list.Rows)
                {
                    object v = row.Cells["col_id"].Value;
                    if (v != null && v.ToString() == selectedId.Value.ToString())
                    {
                        dgv_quotation_list.CurrentCell = row.Cells["col_client_name"];
                        break;
                    }
                }
            }

            if (firstRow >= 0 && firstRow < dgv_quotation_list.Rows.Count)
            {
                try { dgv_quotation_list.FirstDisplayedScrollingRowIndex = firstRow; }
                catch (InvalidOperationException) { /* no rows displayed yet */ }
            }
        }

        private async void SalesQuotationList_Load(object sender, EventArgs e)
        {
            await LoadQuotationList();
        }

        private async Task LoadQuotationList()
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_quotation_list);

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
            ApplySearch();
        }

        // The search filter, applied to whatever _quotationTable currently holds - so a
        // pushed list keeps the engineer's search rather than dropping back to everything.
        private void ApplySearch()
        {
            if (_quotationTable == null || _quotationTable.Rows.Count == 0)
            {
                dgv_quotation_list.DataSource = _quotationTable;
                return;
            }

            // The placeholder text is not a search.
            string searchText = txt_search.Text.Trim();
            if (string.Equals(searchText, "SEARCH BAR", StringComparison.OrdinalIgnoreCase))
                searchText = string.Empty;

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

        // Opens the quotation detail editor - Size Up/item table/wiring interactive,
        // everything else round-trips unchanged on save (see SalesQuotationEngPage's own
        // type-level comment for why enforcement lives there and not on screen).
        private void dgv_quotation_list_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgv_quotation_list.Rows[e.RowIndex];
            int quotationId = Convert.ToInt32(row.Cells["col_id"].Value ?? 0);
            if (quotationId <= 0) return;

            var mainForm = this.FindForm() as SMPC;
            if (mainForm == null) return;

            mainForm.OpenRoute("Sales Quotation Detail");

            foreach (Control ctrl in mainForm.tabContainer.SelectedTab.Controls)
            {
                if (ctrl is smpc_engineering_app.Pages.SalesQuotationEngineering.SalesQuotationEngPage uc)
                {
                    uc.SetQuotation(
                        quotationId,
                        row.Cells["col_client_name"].Value?.ToString(),
                        row.Cells["col_document_no"].Value?.ToString(),
                        row.Cells["col_project_name"].Value?.ToString(),
                        row.Cells["col_sales_executive"].Value?.ToString(),
                        row.Cells["col_status"].Value?.ToString());
                    break;
                }
            }
        }
    }
}
