using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Services.Setup;
using smpc_engineering_app.Services.Helpers;
using System.Diagnostics;
using System.IO;
using smpc_engineering_app.Models;

namespace smpc_engineering_app.Pages.Transactions
{
    public partial class JobOrder : UserControl
    {
        readonly JobOrderService jobOrderService = new JobOrderService();
        readonly EngrUsersService engrUsersService = new EngrUsersService();
        private bool isLoading = false;

        public JobOrder()
        {
            InitializeComponent();
            Helpers.Placeholder.SetPlaceholder(txt_search, "SEARCH BAR");
            tab_container.TabPages.Remove(ongoing_page);
            tab_container.TabPages.Remove(finish_page);
        }

        //Search function
        private void txt_search_TextChanged(object sender, EventArgs e)
        {
            Helpers.ApplySearchFilterRecursive(dgv_pending, txt_search.Text, "type", "item_desc", "date", "sales_order", "materials", "quantity", "due", "a_engr");
            Helpers.ApplySearchFilterRecursive(dgv_ongoing, txt_search.Text, "type", "item_desc", "date", "sales_order", "materials", "quantity", "item_rqst", "status", "due");
            Helpers.ApplySearchFilterRecursive(dgv_finish, txt_search.Text, "general_name", "item_desc", "date", "sales_order", "serial_no", "quantity", "due", "a_engr", "report");
        }

        // Load the data
        private async void JobOrder_Load(object sender, EventArgs e)
        {
            try
            {
                Helpers.Loading.ShowLoading(dgv_pending, "Fetching job orders...");

                await RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while loading the job orders:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                Helpers.Loading.HideLoading(dgv_pending);
                tab_container.TabPages.Add(ongoing_page);
                tab_container.TabPages.Add(finish_page);
            }
        }

        //Hide the columns that are not needed
        private void dgv_ongoing_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv.Columns.Contains("job_order_id2"))
            {
                dgv.Columns["job_order_id2"].Visible = false;
            }

            PrefixSpecialColumns(dgv);
        }

        //Hide the columns that are not needed
        private void dgv_finish_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv.Columns.Contains("job_order_id3"))
            {
            dgv.Columns["job_order_id3"].Visible = false;
            }

            PrefixSpecialColumns(dgv);
        }

        //Hide the columns that are not needed
        private void dgv_pending_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var dgv = sender as DataGridView;
           

            // Mark materials as INCOMPLETE if empty
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (!row.IsNewRow)
                {
                    var cell = row.Cells["materials"];
                    if (cell != null && string.IsNullOrWhiteSpace(cell.Value?.ToString()))
                    {
                        cell.Value = "INCOMPLETE";
                    }
                }
            }

            PrefixSpecialColumns(dgv);
        }

        //Print method in the serial no.
        private async void dgv_finish_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // Handle printing serial_no
            if (dgv_finish.Columns[e.ColumnIndex].Name == "serial_no")
            {
                var cellValue = dgv_finish.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                if (!string.IsNullOrEmpty(cellValue))
                {
                    Helpers.Print.PrintText(cellValue, true);
                }
                else
                {
                    MessageBox.Show("No data to print.");
                }
            }

            if (dgv_finish.Columns[e.ColumnIndex].Name == "sales_order3")
            {
                string cellValue = dgv_finish.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();

                if (!string.IsNullOrEmpty(cellValue))
                {
                    // Remove "SO#"
                    string cleanedValue = cellValue.StartsWith("SO#") ? cellValue.Substring(3) : cellValue;

                    // Debug: show cleaned value
                    Debug.WriteLine($"Passing value: {cleanedValue}");

                    // Pass directly to another UserControl
                    PassSalesOrder(cleanedValue);
                }
            }

            // Handle uploading report
            if (dgv_finish.Columns[e.ColumnIndex].Name == "report")
            {
                string filePath = Helpers.PromptUploadFile();

                if (!string.IsNullOrEmpty(filePath))
                {
                    string fileName = Path.GetFileName(filePath);

                    // Convert file to Base64
                    string fileBase64 = Helpers.ConvertFileToBase64(filePath);

                    // Show filename in the DataGridView cell
                    dgv_finish.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = fileName;

                    var row = dgv_finish.Rows[e.RowIndex];

                    // Build dictionary of row data
                    var data = new Dictionary<string, object>();
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.OwningColumn != null)
                        {
                            string colName = cell.OwningColumn.Name;
                            data[colName] = cell.Value;
                        }
                    }

                    // Fix job_order_id column mapping
                    if (data.ContainsKey("job_order_id3"))
                    {
                        data["job_order_id"] = data["job_order_id3"];
                        data.Remove("job_order_id3");
                    }

                    // Add Base64 file content to report_base column
                    data["report_base"] = fileBase64;

                    // Save to DB
                    var response = await jobOrderService.Save(data);

                    Debug.WriteLine($"Updating row with ID: {data["job_order_id"]}");

                    if (response?.success ?? false)
                    {
                        Debug.WriteLine("Row updated successfully.");
                        MessageBox.Show("File uploaded saved successfully.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to update the report/serial_no field.", "Update Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        //Add S0# or IREQ# in the data
        private void PrefixSpecialColumns(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()))
                        continue;

                    string colName = dgv.Columns[cell.ColumnIndex].Name;

                    if (colName == "sales_order1" || colName == "sales_order2" || colName == "sales_order3")
                    {
                        if (!cell.Value.ToString().StartsWith("SO#"))
                        {
                            cell.Value = "SO#" + cell.Value.ToString();
                        }
                    }
                    else if (colName == "item_rqst1" || colName == "item_rqst2"|| colName == "item_rqst3")
                    {
                        if (!cell.Value.ToString().StartsWith("IREQ#"))
                        {
                            cell.Value = "IREQ#" + cell.Value.ToString();
                        }
                    }
                }
            }
        }

        // Validate the due field
        private void dgv_pending_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (dgv_pending.Columns[e.ColumnIndex].Name == "due")
            {
                var newValue = e.FormattedValue.ToString();

                if (!string.IsNullOrWhiteSpace(newValue))
                {
                    // Try to parse with correct format "MM/dd/yyyy"
                    if (!DateTime.TryParseExact(newValue, "MM/dd/yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out DateTime dueDate))
                    {
                        MessageBox.Show("Invalid date format. Please use MM/dd/yyyy.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true; // Prevent leaving the cell
                    }
                    else
                    {
                        // Compare with current date
                        if (dueDate.Date < DateTime.Today)
                        {
                            MessageBox.Show("Due date cannot be earlier than today.", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            e.Cancel = true;
                        }
                    }
                }
            }
        }

        private DataView FilterPending(DataTable dt)
        {
            if (!dt.Columns.Contains("bom_id"))
                return dt.DefaultView;

            // Filter rows where bom_id is not null and not 0
            var validRows = dt.AsEnumerable()
                .Where(row => row["bom_id"] != null &&
                             !string.IsNullOrWhiteSpace(row["bom_id"].ToString()) &&
                             row["bom_id"].ToString() != "0");

            DataTable resultTable = validRows.Any() ? validRows.CopyToDataTable() : dt.Clone();
            return resultTable.DefaultView;
        }

        // Filter the data rows in the Ongoing page
        private DataView FilterOngoing(DataTable dt)
        {
            if (!dt.Columns.Contains("due") || !dt.Columns.Contains("a_engr"))
                return dt.DefaultView;

            // Make a copy so original data stays intact
            DataTable copy = dt.Copy();

            // Ensure "status" column exists
            if (!copy.Columns.Contains("status"))
            {
                copy.Columns.Add("status", typeof(string));
            }

            // Loop through rows and apply filter + auto set status
            var rowsToRemove = new List<DataRow>();

            foreach (DataRow row in copy.Rows)
            {
                string due = row["due"]?.ToString();
                string engr = row["a_engr"]?.ToString();

                if (string.IsNullOrWhiteSpace(due) || string.IsNullOrWhiteSpace(engr))
                {
                    // Doesn't meet condition -> mark for removal
                    rowsToRemove.Add(row);
                }
            }

            // Remove invalid rows
            foreach (var row in rowsToRemove)
            {
                copy.Rows.Remove(row);
            }

            return copy.DefaultView; // DataView with only valid rows
        }

        // Filter the data rows in the Finished page
        private DataView FilterFinish(DataTable dt)
        {
            if (!dt.Columns.Contains("status"))
                return dt.DefaultView;

            // Make a copy so original data stays intact
            DataTable copy = dt.Copy();

            // Filter rows where status is "complete" (case-insensitive)
            var validRows = copy.AsEnumerable()
                .Where(row => row["status"] != null
                              && row["status"].ToString().Trim().Equals("complete", StringComparison.OrdinalIgnoreCase));

            // Keep only rows with status "complete"
            DataTable resultTable = validRows.Any() ? validRows.CopyToDataTable() : copy.Clone();

            return resultTable.DefaultView;
        }

        // Reload the page
        private async Task RefreshData()
        {
            try
            {
                isLoading = true;

                // Find the index of your existing column
                if (dgv_pending.Columns.Contains("a_engr"))
                {
                    int colIndex = dgv_pending.Columns["a_engr"].Index;

                    DataGridViewComboBoxColumn comboCol = new DataGridViewComboBoxColumn();
                    comboCol.HeaderText = "ASSIGNED ENGR.";
                    comboCol.Name = "a_engr";
                    comboCol.MinimumWidth = 200;
                    comboCol.DataPropertyName = "a_engr";
                    var engrList = await engrUsersService.GetUsersForComboBox();
                    comboCol.Items.AddRange(engrList.ToArray());

                    comboCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    // Replace the column safely
                    dgv_pending.Columns.RemoveAt(colIndex);
                    dgv_pending.Columns.Insert(colIndex, comboCol);
                }
                else
                {
                    MessageBox.Show("Column 'a_engr' not found in dgv_pending.");
                }

                dgv_pending.AutoGenerateColumns = false;
                dgv_ongoing.AutoGenerateColumns = false;
                dgv_finish.AutoGenerateColumns = false;
                try
                {
                    // Await the Task<DataTable>
                    DataTable dt = await jobOrderService.GetAsDatatable();

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No pending data found.");
                        return;
                    }

                    // FormatDate Helper
                    foreach (DataRow row in dt.Rows)
                    {
                        if (dt.Columns.Contains("date"))
                        {
                            row["date"] = Helpers.DateHelper.FormatDate(row["date"]);
                        }
                    }

                    // FormatDate Helper
                    foreach (DataRow row in dt.Rows)
                    {
                        if (dt.Columns.Contains("due"))
                        {
                            row["due"] = Helpers.DateHelper.FormatDate(row["due"]);
                        }
                    }

                    Debug.WriteLine(dt);

                    // Bind to BindingSource
                    dgv_pending.DataSource = FilterPending(dt);
                    dgv_ongoing.DataSource = FilterOngoing(dt);
                    dgv_finish.DataSource = FilterFinish(dt);

                    // Debug preview in Output window
                    Debug.WriteLine($"Fetched {dt.Rows.Count} rows.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching data: {ex.Message}");
                }
            } finally
            {
                isLoading = false;
            } 
        }

        // Refresh the data upon changing of pages
        private async void tab_container_SelectedIndexChanged(object sender, EventArgs e)
        {
            await RefreshData();
        }

        // Center the drop down choices
        private void dgv_pending_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgv_pending.CurrentCell.OwningColumn.Name == "a_engr" && e.Control is ComboBox combo)
            {
                // Center align the dropdown items
                combo.DrawMode = DrawMode.OwnerDrawFixed;
                combo.DrawItem -= Combo_DrawItem;
                combo.DrawItem += Combo_DrawItem;
            }
        }

        // Center the drop down choices
        private void Combo_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ComboBox combo = sender as ComboBox;

            // Background
            e.DrawBackground();

            // Get item text
            string text = combo.Items[e.Index].ToString();

            // Measure string size
            var textSize = e.Graphics.MeasureString(text, e.Font);

            // Calculate center position
            float x = e.Bounds.Left + (e.Bounds.Width - textSize.Width) / 2;
            float y = e.Bounds.Top + (e.Bounds.Height - textSize.Height) / 2;

            // Draw centered text
            using (Brush textBrush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(text, e.Font, textBrush, x, y);
            }

            // Draw focus rectangle if needed
            e.DrawFocusRectangle();
        }

        private async void dgv_pending_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            await CreateOrUpdateJobOrderPending(e.RowIndex);
        }

        private async void dgv_ongoing_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            await UpdateJobOrderOngoing(e.RowIndex);
        }

        private async void dgv_finish_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            await UpdateJobOrderFinish(e.RowIndex);
        }

        private async Task UpdateJobOrderFinish(int rowIndex)
        {
            if (isLoading || rowIndex < 0) return; // ignore header row

            await Helpers.UpdateRowAsync<JobOrderModel>(
                dgv_finish,
                rowIndex,
                "job_order_id3",
                async (job_order_id, data) =>
                {
                    if (data.ContainsKey("job_order_id3"))
                    {
                        data["job_order_id"] = data["job_order_id3"];
                        data.Remove("job_order_id3");
                    }

                    var response = await jobOrderService.Save(data);

                    Debug.WriteLine($"Updating row with ID: {job_order_id}");

                    return (response.success, default(JobOrderModel));
                });

            await RefreshData();
        }

        private async Task UpdateJobOrderOngoing(int rowIndex)
        {
            if (isLoading || rowIndex < 0) return; // ignore header row

            await Helpers.UpdateRowAsync<JobOrderModel>(
                dgv_ongoing,
                rowIndex,
                "job_order_id2",
                async (job_order_id, data) =>
                {
                    if (data.ContainsKey("job_order_id2"))
                    {
                        data["job_order_id"] = data["job_order_id2"];
                        data.Remove("job_order_id2");
                    }

                    var response = await jobOrderService.Save(data);

                    Debug.WriteLine($"Updating row with ID: {job_order_id}");

                    return (response.success, default(JobOrderModel));
                });

            await RefreshData();
        }

        private async Task CreateOrUpdateJobOrderPending(int rowIndex)
        {
            if (isLoading || rowIndex < 0) return; // ignore header row

            await Helpers.UpdateRowAsync<JobOrderModel>(
                dgv_pending,
                rowIndex,
                "job_order_id1",
                async (job_order_id, data) =>
                {
                    // Create a new dictionary with cleaned keys (remove trailing '1')
                    var cleanedData = data.ToDictionary(
                                kv => kv.Key.EndsWith("1") ? kv.Key.Substring(0, kv.Key.Length - 1) : kv.Key,
                                kv => kv.Value
                            );

                    // Ensure "id" exists correctly
                    if (cleanedData.ContainsKey("job_order_id"))
                        job_order_id = cleanedData["job_order_id"]?.ToString();

                    var response = await jobOrderService.Save(cleanedData);

                    Debug.WriteLine($"Updating row with ID: {job_order_id}");
                    foreach (var kv in cleanedData)
                        Debug.WriteLine($"{kv.Key}: {kv.Value}");

                    return (response.success, default(JobOrderModel));
                });

            await RefreshData();
        }

        private void dgv_pending_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string columnName = dgv_pending.Columns[e.ColumnIndex].Name;
            string cellValue = dgv_pending.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();

            if (columnName == "sales_order1")
            {
                if (!string.IsNullOrEmpty(cellValue))
                {
                    // Remove "SO#"
                    string cleanedValue = cellValue.StartsWith("SO#") ? cellValue.Substring(3) : cellValue;

                    // Debug: show cleaned value
                    Debug.WriteLine($"Passing value: {cleanedValue}");

                    // Pass directly to another UserControl
                    PassSalesOrder(cleanedValue);
                }
            }
            else if (columnName == "materials")
            {
                var row = dgv_pending.Rows[e.RowIndex];

                string bomId = row.Cells["bom_id1"].Value?.ToString();

                // validate
                if (string.IsNullOrEmpty(bomId) || bomId == "0")
                {
                    MessageBox.Show("No BOM available for this row.",
                                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var form = new MaterialsForm(bomId))
                {
                    form.ShowDialog(this);
                }
            }
        }

        // This helper method looks for Sales Order and passes the value
        private void PassSalesOrder(string salesOrder)
        {
            var mainForm = this.FindForm() as SMPC;
            if (mainForm == null) return;

            TabPage salesOrderTab = null;

            // Check if tab already exists
            foreach (TabPage tab in mainForm.tabContainer.TabPages)
            {
                if (tab.Name == "Sales Order")
                {
                    salesOrderTab = tab;
                    break;
                }
            }

            // If not found, create/open it like TreeView node does
            if (salesOrderTab == null)
            {
                salesOrderTab = mainForm.OpenSalesOrderTab();
            }

            // Now look for your SalesOrder UC inside that tab
            foreach (Control ctrl in salesOrderTab.Controls)
            {
                if (ctrl is SalesOrder salesOrderUC)
                {
                    salesOrderUC.SetSalesOrder(salesOrder);
                    break;
                }
            }

            // Optionally switch to the tab
            mainForm.tabContainer.SelectedTab = salesOrderTab;
        }

        private void dgv_ongoing_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string columnName = dgv_ongoing.Columns[e.ColumnIndex].Name;
            string cellValue = dgv_ongoing.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();

            if (columnName == "sales_order2")
            {
                if (!string.IsNullOrEmpty(cellValue))
                {
                    // Remove "SO#"
                    string cleanedValue = cellValue.StartsWith("SO#") ? cellValue.Substring(3) : cellValue;

                    // Debug: show cleaned value
                    Debug.WriteLine($"Passing value: {cleanedValue}");

                    // Pass directly to another UserControl
                    PassSalesOrder(cleanedValue);
                }
            }
        }

        private async void dgv_ongoing_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var dgv = sender as DataGridView;
            var row = dgv.Rows[e.RowIndex];
            var colName = dgv.Columns[e.ColumnIndex].Name;

            // Only act if the status column changed
            if (colName == "status")
            {
                var statusValue = row.Cells["status"].Value?.ToString()?.Trim().ToLower();

                if (statusValue == "complete")
                {
                    var serialCell = row.Cells["serial_no1"];
                    if (serialCell != null && string.IsNullOrWhiteSpace(serialCell.Value?.ToString()))
                    {
                        // Generate a 10-digit serial number
                        serialCell.Value = Helpers.RandomNumber.Generate10DigitNumber();

                        // Build dictionary of row data
                        var data = new Dictionary<string, object>();
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell.OwningColumn != null)
                            {
                                // Remove trailing '1' from column names before saving
                                string key = cell.OwningColumn.Name;
                                if (key.EndsWith("1"))
                                    key = key.Substring(0, key.Length - 1);

                                data[key] = cell.Value;
                            }
                        }

                        // Fix job_order_id column if still present
                        if (data.ContainsKey("job_order_id2"))
                        {
                            data["job_order_id"] = data["job_order_id2"];
                            data.Remove("job_order_id2");
                        }

                        var response = await jobOrderService.Save(data);

                        if (!response?.success ?? false)
                        {
                            MessageBox.Show("Failed to update serial number.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
        }

    }
}
