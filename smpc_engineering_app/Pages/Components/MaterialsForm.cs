using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Services.Setup;
using smpc_engineering_app.Services.Helpers;
using smpc_engineering_app.Services;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;

namespace smpc_engineering_app.Pages
{
    public partial class MaterialsForm : Form
    {
        GeneralService<ComponentModel> generalgetComponents;
        private readonly string _bomId;
        private readonly string _soId;

        // soId is optional (defaults "0") for any existing caller that doesn't have
        // it handy - the API still returns a real stock figure without it, just
        // without the carve-out for this job's own SO's own approved reservation
        // (see sp_GetComponents.sql's header). JobOrderPage always has it and
        // always passes it.
        public MaterialsForm(string bomId, string soId = "0")
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

            _bomId = bomId;
            _soId = string.IsNullOrWhiteSpace(soId) ? "0" : soId;
        }

        private async void MaterialsForm_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_components, "Fetching components...");

                generalgetComponents = new GeneralService<ComponentModel>(
                    ApiEndPoints.COMPONENTS + "/" + _bomId + "?so_id=" + _soId);
                DataTable dt = await generalgetComponents.GetAsDatatable();

                dgv_components.DataSource = dt;
            }
            catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "No components found.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load components.\n{ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_components);
            }
        }

        private void dgv_components_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;

            // Ensure the numbering column exists
            if (grid.Columns.Contains("numbering"))
            {
                grid.Rows[e.RowIndex].Cells["numbering"].Value = (e.RowIndex + 1).ToString();
            }
        }
    }
}
