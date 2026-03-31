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

namespace smpc_engineering_app.Pages.PickActivity.PickActivityModals
{
    public partial class PickActivitySearch : Form
    {
        public string SelectedPAId { get; private set; } = null;
        private string placeHolderText = "Pick Activity Search...";
        private PickActivityList PickActivity;
        readonly PickActivityService pickActivityService = new PickActivityService();
        private DataTable paTable;

        public PickActivitySearch()
        {
            InitializeComponent();

            // Center the modal relative to its parent form
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeSearchBox();
        }

        private void InitializeSearchBox()
        {
            txt_search = Helpers.CreateSearchBox(placeHolderText, txt_search_TextChanged);
            this.Controls.Add(txt_search);
        }

        private void txt_search_TextChanged(object sender, EventArgs e)
        {
            if (paTable == null || paTable.Rows.Count == 0)
                return;

            string searchText = txt_search.Text.Trim();

            if (string.IsNullOrEmpty(searchText) || searchText == placeHolderText)
            {
                dgv_pa_search.DataSource = paTable;
            }
            else
            {
                var searchedData = Helpers.FilterDataTable(paTable, searchText, "doc_no", "customer", "reference_so", "sales_person", "prepared_by", "picked_by");
                dgv_pa_search.DataSource = searchedData;
            }
        }

        private async void PickActivitySearch_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_pa_search, "Fetching data...");
                await PickActivities();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_pa_search);
            }
        }

        private async Task PickActivities()
        {
            try
            {
                PickActivity = await pickActivityService.GetAsModel();

                PickActivity.pick_activity.Reverse();

                // Convert pick activity list to DataTable using helper
                paTable = Helpers.ToDataTable(PickActivity.pick_activity);

                if (paTable?.Rows.Count > 0)
                {
                    dgv_pa_search.DataSource = paTable;
                }
                else
                {
                    dgv_pa_search.DataSource = null;
                    Helpers.ShowDialogMessage("error", "No pick activity found.");
                }
            }
            catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "No pick activity found.");
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
        }

        private void dgv_pa_search_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var row = dgv_pa_search.Rows[e.RowIndex];

            // Always get the id value from the row, regardless of which column was clicked
            var idValue = row.Cells["id"].Value;

            if (idValue != null)
            {
                SelectedPAId = idValue.ToString();

                this.DialogResult = DialogResult.OK; // close the modal with OK
                this.Close();
            }
        }
    }
}
