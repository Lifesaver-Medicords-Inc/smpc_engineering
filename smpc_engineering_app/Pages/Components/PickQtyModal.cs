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

namespace smpc_engineering_app.Pages.Components
{
    public partial class PickQtyModal : Form
    {
        //Dictionaries for the column grouping of datagridviews
        Dictionary<string, string[]> columnGroupsMain = new Dictionary<string, string[]>()
        {
            { "PICK", new string[] { "issued_qty", "issued_uom" } },
            { "STOCK", new string[] { "stock_qty", "stock_uom" } },
        };

        public string PassedUom { get; set; }
        public int PassedId { get; set; }
        public int PassedItemId { get; set; }
        public DataTable PassedIRLocation { get; set; }
        public DataTable SelectedIssuedLocations { get; private set; }
        public int TotalIssuedQty { get; private set; }
        readonly BinLocationListService binLocationService = new BinLocationListService();
        private DataTable locationTable;
        public PickQtyModal()
        {
            InitializeComponent();

            // Center the modal relative to its parent form
            this.StartPosition = FormStartPosition.CenterParent;
            Helpers.EnableGroupHeaders(dgv_pick_qty, columnGroupsMain);
        }

        private async void PickQtyModal_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_pick_qty, "Fetching data...");
                await LoadBinLocations();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_pick_qty);

            }
        }

        private async Task LoadBinLocations()
        {
            try
            {
                var data = await binLocationService.GetFilteredLocation(PassedItemId);
                locationTable = data;

                if (locationTable.Rows.Count > 0)
                {
                    // Keep only unique rows based on location + warehouse_id
                    locationTable = locationTable.AsEnumerable()
                        .GroupBy(row => new
                        {
                            Location = row["location"]?.ToString()?.Trim(),
                            WarehouseId = row["warehouse_id"]?.ToString()?.Trim()
                        })
                        .Select(g => g.First()) // keep the first unique combination
                        .CopyToDataTable();


                    //Set each row's issued_uom to the passed UOM
                    foreach (DataRow row in locationTable.Rows)
                    {
                        row["issued_uom"] = PassedUom;
                        row["stock_uom"] = PassedUom;
                        row["ir_details_id"] = PassedId;
                    }

                    ApplyPassedIRLocation();

                    dgv_pick_qty.DataSource = locationTable;
                }
                else
                {
                    dgv_pick_qty.DataSource = null;
                    MessageBox.Show("No item list found.");
                }
            } catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "The item has no available bin location yet.");
                this.Close();
            }
        }

        private void ApplyPassedIRLocation()
        {
            if (PassedIRLocation == null || PassedIRLocation.Rows.Count == 0)
                return;

            foreach (DataRow passedRow in PassedIRLocation.Rows)
            {
                string passedLocation = passedRow["location"]?.ToString()?.Trim();
                string passedWarehouseId = passedRow["warehouse_id"]?.ToString()?.Trim();
                string passedIssuedQty = passedRow["issued_qty"]?.ToString()?.Trim();

                if (string.IsNullOrEmpty(passedLocation) || string.IsNullOrEmpty(passedWarehouseId))
                    continue;

                // Find matching rows in the main locationTable
                var matches = locationTable.AsEnumerable()
                    .Where(row =>
                        string.Equals(row["location"]?.ToString()?.Trim(), passedLocation, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(row["warehouse_id"]?.ToString()?.Trim(), passedWarehouseId, StringComparison.OrdinalIgnoreCase)
                    );

                foreach (var match in matches)
                {
                    // Set issued_qty to the one from PassedIRLocation
                    match["issued_qty"] = passedIssuedQty;
                }
            }
        }

        private void dgv_pick_qty_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // Get the name of the current column
            string columnName = dgv_pick_qty.Columns[dgv_pick_qty.CurrentCell.ColumnIndex].Name;

            // List of column names that should accept only numbers
            string[] numericColumns = { "issued_qty" };

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

        private void btn_save_Click(object sender, EventArgs e)
        {
            try
            {
                int total = 0;

                // Get the current table bound to the DataGridView
                DataTable sourceTable = dgv_pick_qty.DataSource as DataTable;

                if (sourceTable == null)
                {
                    Helpers.ShowDialogMessage("warning", "No data available.");
                    return;
                }

                // Clone structure for storing only rows with issued_qty
                SelectedIssuedLocations = sourceTable.Clone();

                foreach (DataRow row in sourceTable.Rows)
                {
                    var qtyValue = row["issued_qty"]?.ToString()?.Trim();

                    if (decimal.TryParse(qtyValue, out decimal qty) && qty > 0)
                    {
                        // Add to total
                        total += (int)qty;

                        // Copy row to the filtered table
                        SelectedIssuedLocations.ImportRow(row);
                    }
                }

                TotalIssuedQty = total;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Error while saving: {ex.Message}");
            }
        }
    }
}
