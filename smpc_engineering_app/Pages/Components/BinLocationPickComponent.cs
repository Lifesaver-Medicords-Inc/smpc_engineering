using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Services;
using smpc_engineering_app.Models;
using smpc_engineering_app.Services.Helpers;
using smpc_engineering_app.Shared;

namespace smpc_engineering_app.Pages.Components
{
    public partial class BinLocationPickComponent : Form
    {
        private int _itemID;
        GeneralService<BinLocation2Model> binLocationServiceSetup;
        private List<BinLocation2Model> BinLocation;
        private DataTable blTable;
        // Add this public property to hold the computed sum
        public int TotalSelectedQty { get; private set; }
        public List<SelectedBinLocation> SelectedLocations { get; private set; } = new List<SelectedBinLocation>();

        // Add this nested class at the bottom of the file (or in a Models file)
        public class SelectedBinLocation
        {
            public int BinId { get; set; }
            public int SelectedQty { get; set; }
        }

        public BinLocationPickComponent(int itemId)
        {
            InitializeComponent();

            // Center the modal relative to its parent form
            this.StartPosition = FormStartPosition.CenterParent;

            dgv_main.AutoGenerateColumns = false;

            _itemID = itemId;

            dgv_main.AutoGenerateColumns = false;
        }

        private async void BinLocationPickComponent_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_main, "Fetching data...");
                await LoadBinLocations();
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

        private async Task LoadBinLocations()
        {
            try
            {
                binLocationServiceSetup = new GeneralService<BinLocation2Model>(ApiEndPoints.BIN_LOCATIONS + _itemID);
                BinLocation = await binLocationServiceSetup.GetAsList();

                // Convert bin location list to DataTable using helper
                blTable = Helpers.ToDataTable(BinLocation);

                if (blTable?.Rows.Count > 0)
                {
                    dgv_main.DataSource = blTable;
                }
                else
                {
                    dgv_main.DataSource = null;
                    Helpers.ShowDialogMessage("error", "No bin location found.");
                }
            }
            catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "No bin location found.");
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
        }

        private void dgv_main_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgv_main.Columns[e.ColumnIndex].Name == "IsSelected")
            {
                DataGridViewRow row = dgv_main.Rows[e.RowIndex];
                bool isChecked = Convert.ToBoolean(row.Cells["IsSelected"].Value);

                DataGridViewCell qtyCell = row.Cells["selected_Qty"];

                if (isChecked)
                {
                    qtyCell.ReadOnly = false;
                    qtyCell.Style.BackColor = Color.White;
                }
                else
                {
                    qtyCell.ReadOnly = true;
                    qtyCell.Style.BackColor = Color.Gainsboro; // visual cue for readonly
                    qtyCell.Value = null; // optional: clear value when unchecked
                }
            }
        }

        private void dgv_main_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgv_main.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgv_main.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            int total = 0;
            var selections = new List<SelectedBinLocation>();

            foreach (DataGridViewRow row in dgv_main.Rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells["IsSelected"].Value);

                if (!isChecked) continue;

                int selectedQty = 0;
                int stockQty = 0;

                bool selectedValid = int.TryParse(row.Cells["selected_Qty"].Value?.ToString(), out selectedQty);
                bool stockValid = int.TryParse(row.Cells["stock_qty"].Value?.ToString(), out stockQty);

                if (!selectedValid || selectedQty <= 0)
                {
                    Helpers.ShowDialogMessage("error", $"Row {row.Index + 1}: Please enter a valid quantity.");
                    dgv_main.CurrentCell = row.Cells["selected_Qty"];
                    return;
                }

                if (selectedQty > stockQty)
                {
                    Helpers.ShowDialogMessage("error", $"Row {row.Index + 1}: Selected quantity ({selectedQty}) cannot exceed stock quantity ({stockQty}).");
                    dgv_main.CurrentCell = row.Cells["selected_Qty"];
                    return;
                }

                total += selectedQty;

                // Collect bin_id alongside qty
                if (int.TryParse(row.Cells["bin_id"].Value?.ToString(), out int binId))
                {
                    selections.Add(new SelectedBinLocation
                    {
                        BinId = binId,
                        SelectedQty = selectedQty
                    });
                }
            }

            // Check at least one row was selected
            if (total <= 0)
            {
                Helpers.ShowDialogMessage("error", "Please select at least one bin location and enter a quantity.");
                return;
            }

            // Store the sum and close with OK result
            TotalSelectedQty = total;
            SelectedLocations = selections;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void dgv_main_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            Helpers.HandleNumericColumns(dgv_main, e, new[] { "selected_qty" });
        }
    }
}
