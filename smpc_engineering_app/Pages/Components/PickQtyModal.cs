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
using smpc_engineering_app.Services.Transaction;
using smpc_engineering_app.Models;

namespace smpc_engineering_app.Pages.Components
{
    public partial class PickQtyModal : Form
    {
        public string PassedUom { get; set; }
        public int PassedIRId { get; set; }
        public int PassedIRParentId { get; set; }
        public int PassedPAId { get; set; }
        public int PassedItemId { get; set; }
        public bool IsNewMode { get; set; }
        public DataTable PassedIRLocation { get; set; }
        public DataTable PassedPALocation { get; set; }
        public DataTable SelectedIssuedLocations { get; private set; }
        public DataTable SelectedActualLocations { get; private set; }
        public int TotalIssuedQty { get; private set; }
        public int TotalActualQty { get; private set; }
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
                locationTable = data ?? new DataTable();

                // Apply IR Passed Locations
                if (PassedIRLocation != null && PassedIRLocation.Rows.Count > 0)
                    ApplyPassedLocation(PassedIRLocation, "issued_qty");

                // Apply PA Passed Locations
                if (PassedPALocation != null && PassedPALocation.Rows.Count > 0)
                    ApplyPassedLocation(PassedPALocation, "actual_qty");

                if (locationTable.Rows.Count > 0)
                {
                    //Filter out rows where stock_qty <= 0 or stock_qty is invalid
                    var filteredRows = locationTable.AsEnumerable()
                        .Where(row =>
                        {
                            var stockVal = row["stock_qty"]?.ToString()?.Trim();
                            return decimal.TryParse(stockVal, out decimal stockQty) && stockQty > 0;
                        })
                        .ToList();

                    if (filteredRows.Count == 0)
                    {
                        Helpers.ShowDialogMessage("error", "No locations found with stock greater than 0.");
                        dgv_pick_qty.DataSource = null;
                        return;
                    }

                    //Keep only unique rows based on location + warehouse_id
                    locationTable = filteredRows
                        .GroupBy(row => new
                        {
                            Location = row["location"]?.ToString()?.Trim(),
                            WarehouseId = row["warehouse_id"]?.ToString()?.Trim()
                        })
                        .Select(g =>
                        {
                            // Create a new row based on first row structure
                            var newRow = g.First().Table.NewRow();

                            foreach (DataColumn col in g.First().Table.Columns)
                                newRow[col.ColumnName] = g.First()[col.ColumnName];

                            // SUM stock_qty of grouped rows
                            decimal totalStock = g.Sum(r =>
                            {
                                var val = r["stock_qty"].ToString().Trim();
                                return decimal.TryParse(val, out decimal qty) ? qty : 0;
                            });

                            newRow["stock_qty"] = totalStock;

                            return newRow;
                        })
                        .CopyToDataTable();


                    //Set UOM and related fields
                    foreach (DataRow row in locationTable.Rows)
                    {
                        row["issued_uom"] = PassedUom;
                        row["actual_uom"] = PassedUom;
                        row["stock_uom"] = PassedUom;
                        row["ir_details_id"] = PassedIRId;
                        row["pa_details_id"] = PassedPAId;
                        row["item_id"] = PassedItemId;
                        row["ir_id"] = PassedIRParentId;
                    }

                    // After subtraction
                    // Refilter rows with stock_qty > 0
                    var finalFilteredRows = locationTable.AsEnumerable()
                        .Where(row =>
                        {
                            var stockVal = row["stock_qty"]?.ToString()?.Trim();
                            return decimal.TryParse(stockVal, out decimal sq) && sq > 0;
                        })
                        .ToList();

                    if (finalFilteredRows.Count == 0)
                    {
                        Helpers.ShowDialogMessage("error", "No available stock after allocation.");
                        dgv_pick_qty.DataSource = null;
                        this.Close();
                        return;
                    }

                    dgv_pick_qty.DataSource = finalFilteredRows.CopyToDataTable();

                    //Apply column visibility and grouping rules
                    ApplyColumnVisibilityAndGrouping();
                }
                else
                {
                    dgv_pick_qty.DataSource = null;
                    Helpers.ShowDialogMessage("error", "No item list found.");
                    this.Close();
                }
            }
            catch (NullReferenceException)
            {
                Helpers.ShowDialogMessage("error", "No available stock after allocation.");
                this.Close();
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load: {ex.Message}");
            }
        }

        private void ApplyPassedLocation(DataTable passedTable, string qtyColumn)
        {
            if (passedTable == null || passedTable.Rows.Count == 0)
                return;

            foreach (DataRow passedRow in passedTable.Rows)
            {
                string passedId = passedRow["id"]?.ToString()?.Trim();
                string passedLocation = passedRow["location"]?.ToString()?.Trim();
                string passedWarehouseId = passedRow["warehouse_id"]?.ToString()?.Trim();
                string passedQtyStr = passedRow[qtyColumn]?.ToString()?.Trim();

                if (string.IsNullOrEmpty(passedLocation) || string.IsNullOrEmpty(passedWarehouseId))
                    continue;

                decimal.TryParse(passedQtyStr, out decimal passedQty);

                var matches = locationTable.AsEnumerable()
                    .Where(row =>
                        string.Equals(row["location"]?.ToString()?.Trim(), passedLocation, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(row["warehouse_id"]?.ToString()?.Trim(), passedWarehouseId, StringComparison.OrdinalIgnoreCase)
                    );

                foreach (var match in matches)
                {
                    // Set issued or actual qty
                    match[qtyColumn] = passedQty;

                    // Keep ID
                    match["id"] = passedId;
                }
            }
        }

        private void dgv_pick_qty_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            // Get the name of the current column
            string columnName = dgv_pick_qty.Columns[dgv_pick_qty.CurrentCell.ColumnIndex].Name;

            // List of column names that should accept only numbers
            string[] numericColumns = { "issued_qty", "actual_qty" };

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
                    Helpers.ShowDialogMessage("error", "No data available.");
                    return;
                }

                // Clone structure for storing only rows with issued_qty
                SelectedIssuedLocations = sourceTable.Clone();

                // Determine which column to use based on the context
                string qtyColumn = PassedIRId > 0 ? "issued_qty" : "actual_qty";
                string uomColumn = PassedIRId > 0 ? "issued_uom" : "actual_uom";

                foreach (DataRow row in sourceTable.Rows)
                {
                    var qtyValue = row[qtyColumn]?.ToString()?.Trim();
                    var stockValue = row["stock_qty"]?.ToString()?.Trim();

                    if (decimal.TryParse(qtyValue, out decimal qty) && qty > 0)
                    {
                        if (!decimal.TryParse(stockValue, out decimal stockQty))
                        {
                            Helpers.ShowDialogMessage("error", "Invalid or missing stock quantity.");
                            return;
                        }

                        //qty cannot exceed stock_qty
                        if (qty > stockQty)
                        {
                            Helpers.ShowDialogMessage("error",
                                $"Issued qty ({qty}) cannot exceed stock qty ({stockQty}).");
                            return;
                        }

                        total += (int)qty;
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

        private void ApplyColumnVisibilityAndGrouping()
        {
            try
            {
                bool hasIRId = PassedIRId > 0;
                bool hasPAId = PassedPAId > 0;

                // Reset visibility (make all visible by default)
                dgv_pick_qty.Columns["issued_qty"].Visible = true;
                dgv_pick_qty.Columns["issued_uom"].Visible = true;
                dgv_pick_qty.Columns["actual_qty"].Visible = true;
                dgv_pick_qty.Columns["actual_uom"].Visible = true;

                // If IR ID is provided → show Issued columns, hide Actual columns
                if (hasIRId && !hasPAId)
                {
                    dgv_pick_qty.Columns["actual_qty"].Visible = false;
                    dgv_pick_qty.Columns["actual_uom"].Visible = false;

                    // Group only the issued columns
                    var irColumns = new Dictionary<string, string[]>
                    {
                        { "PICK", new string[] { "issued_qty", "issued_uom" } },
                        { "STOCK", new string[] { "stock_qty", "stock_uom" } },
                    };
                    Helpers.EnableGroupHeaders(dgv_pick_qty, irColumns);
                }
                // If PA ID is provided → show Actual columns, hide Issued columns
                else if (hasPAId && !hasIRId)
                {
                    dgv_pick_qty.Columns["issued_qty"].Visible = false;
                    dgv_pick_qty.Columns["issued_uom"].Visible = false;

                    // Group only the actual columns
                    var paColumns = new Dictionary<string, string[]>
                    {
                        { "PICK", new string[] { "actual_qty", "actual_uom" } },
                        { "STOCK", new string[] { "stock_qty", "stock_uom" } },
                    };
                    Helpers.EnableGroupHeaders(dgv_pick_qty, paColumns);
                }
                // If neither has value → default grouping
                else
                {
                    var defaultColumns = new Dictionary<string, string[]>
                    {
                        { "PICK", new string[] { "issued_qty", "issued_uom" } },
                        { "STOCK", new string[] { "stock_qty", "stock_uom" } },
                    };
                    Helpers.EnableGroupHeaders(dgv_pick_qty, defaultColumns);
                }
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to apply grouping: {ex.Message}");
            }
        }
    }
}
