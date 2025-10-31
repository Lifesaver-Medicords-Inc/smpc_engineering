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
        public string PassedUom { get; set; }
        public int PassedId { get; set; }
        public DataTable PassedIRLocation { get; set; }
        public int TotalIssuedQty { get; private set; }
        readonly BinLocationListService binLocationService = new BinLocationListService();
        private DataTable locationTable;
        public PickQtyModal()
        {
            InitializeComponent();

            // Center the modal relative to its parent form
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private async void PickQtyModal_Load(object sender, EventArgs e)
        {

            Console.WriteLine(PassedIRLocation);
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
            var data = await binLocationService.GetAsDatatable();
            locationTable = ExpandLocationRanges(data);

            if (locationTable.Rows.Count > 0)
            {
                //Set each row's issued_uom to the passed UOM
                foreach (DataRow row in locationTable.Rows)
                {
                    row["issued_uom"] = PassedUom;
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

        private DataTable ExpandLocationRanges(DataTable originalTable)
        {
            DataTable expandedTable = originalTable.Clone(); // copy structure

            foreach (DataRow row in originalTable.Rows)
            {
                string location = row["location"]?.ToString().Trim() ?? "";

                // Check if location contains a range (e.g., "TO")
                if (location.IndexOf(" TO ", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Split start and end parts
                    string[] parts = location.Split(new string[] { " TO " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        string startPart = parts[0].Trim(); // e.g., Z3-A3-R2-L2-B1
                        string endPart = parts[1].Trim();   // e.g., B5

                        // Find the last '-' to separate prefix from bin section
                        int lastDashIndex = startPart.LastIndexOf('-');
                        if (lastDashIndex >= 0)
                        {
                            string prefix = startPart.Substring(0, lastDashIndex + 1); // e.g., Z3-A3-R2-L2-
                            string binPart = startPart.Substring(lastDashIndex + 1);   // e.g., B1

                            // Split binPart into letter and number (e.g., B + 1)
                            char binLetter = binPart[0];
                            string startBinNumber = binPart.Substring(1); // "1"

                            // Get end bin number (strip B if present)
                            string endBinNumber = endPart.StartsWith(binLetter.ToString(), StringComparison.OrdinalIgnoreCase)
                                ? endPart.Substring(1)
                                : endPart;

                            // Try to parse start and end numbers
                            if (int.TryParse(startBinNumber, out int start) && int.TryParse(endBinNumber, out int end))
                            {
                                // Generate rows B1...B5 (preserving B)
                                for (int i = start; i <= end; i++)
                                {
                                    DataRow newRow = expandedTable.NewRow();
                                    newRow.ItemArray = (object[])row.ItemArray.Clone();
                                    newRow["location"] = $"{prefix}{binLetter}{i}";
                                    expandedTable.Rows.Add(newRow);
                                }
                                continue; // next original row
                            }
                        }
                    }
                }

                // If no range detected, keep the row as is
                expandedTable.ImportRow(row);
            }

            return expandedTable;
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

                foreach (DataGridViewRow dgvRow in dgv_pick_qty.Rows)
                {
                    if (dgvRow.IsNewRow) continue;

                    var qtyValue = dgvRow.Cells["issued_qty"].Value?.ToString();

                    if (decimal.TryParse(qtyValue, out decimal qty))
                    {
                        total += (int)qty;
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
