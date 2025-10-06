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

namespace smpc_engineering_app.Pages
{
    public partial class MaterialsForm : Form
    {
        readonly ComponentService componentService = new ComponentService();
        private readonly string _bomId;
        public MaterialsForm(string bomId)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

            _bomId = bomId;        }

        private async void MaterialsForm_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_components, "Fetching components...");

                DataTable dt = await componentService.GetAsDatatable(_bomId);

                dgv_components.DataSource = dt;
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
    }
}
