using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Data;
using System.Data.SqlTypes;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;

namespace smpc_engineering_app.Services.Helpers
{
    public static class Helpers
    {
        public static class DataGridViewDocumentFormatter
        {
            private static readonly Dictionary<DataGridView, Tuple<string, string, int>> _docConfigs
                = new Dictionary<DataGridView, Tuple<string, string, int>>();

            public static void DataGridViewDocumentFormat(DataGridView dgv, string columnName, string prefix, int digits = 8)
            {
                if (dgv == null) return;

                _docConfigs[dgv] = new Tuple<string, string, int>(columnName, prefix, digits);

                dgv.DataBindingComplete -= Dgv_DataBindingComplete;
                dgv.DataBindingComplete += Dgv_DataBindingComplete;

                dgv.CellFormatting -= Dgv_CellFormatting;
                dgv.CellFormatting += Dgv_CellFormatting;
            }

            private static void Dgv_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
            {
                var dgv = sender as DataGridView;
                if (dgv == null || !_docConfigs.ContainsKey(dgv)) return;

                var tag = _docConfigs[dgv];

                if (!dgv.Columns.Contains(tag.Item1)) return;

                dgv.Columns[tag.Item1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }

            private static void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
            {
                var dgv = sender as DataGridView;
                if (dgv == null || !_docConfigs.ContainsKey(dgv)) return;

                var tag = _docConfigs[dgv];

                string columnName = tag.Item1;
                string prefix = tag.Item2;
                int digits = tag.Item3;

                if (dgv.Columns[e.ColumnIndex].Name != columnName) return;
                if (e.Value == null) return;

                if (int.TryParse(e.Value.ToString(), out int number))
                {
                    e.Value = prefix + number.ToString($"D{digits}");
                    e.FormattingApplied = true;
                }
            }
        }

        /// <summary>
        /// Restricts specified DataGridView columns to numeric input only.
        /// </summary>
        public static void HandleNumericColumns(
            DataGridView dgv,
            DataGridViewEditingControlShowingEventArgs e,
            params string[] numericColumnNames)
        {
            if (dgv.CurrentCell == null)
                return;

            string columnName = dgv.Columns[dgv.CurrentCell.ColumnIndex].Name;

            // Always remove first to prevent duplicate handlers
            e.Control.KeyPress -= NumericColumn_KeyPress;

            // Attach only if column is numeric
            if (numericColumnNames.Contains(columnName))
            {
                e.Control.KeyPress += NumericColumn_KeyPress;
            }
        }

        /// <summary>
        /// Allows only digits, control keys, and a single decimal point.
        /// </summary>
        private static void NumericColumn_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Block non-numeric characters
            if (!char.IsControl(e.KeyChar) &&
                !char.IsDigit(e.KeyChar) &&
                e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            // Allow only one decimal point
            if (e.KeyChar == '.' &&
                sender is TextBox tb &&
                tb.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        public static class DatagridviewMapper
        {
            // Model mapper for DataGridView / DataTable
            public static List<T> BuildModelsFromData<T>(object dataSource, HashSet<int> editedRowIndices = null) where T : new()
            {
                var models = new List<T>();
                var modelType = typeof(T);
                var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // --- CASE 1: DataGridView ---
                if (dataSource is DataGridView dgv)
                {
                    if (dgv.Rows.Count == 0)
                        return models;

                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.IsNewRow) continue;

                        // *** CHANGED: If editedRowIndices is provided, only process those rows ***
                        if (editedRowIndices != null && !editedRowIndices.Contains(row.Index))
                            continue;

                        // Check if row has ANY data in mapped columns
                        bool rowHasData = false;
                        foreach (var prop in properties)
                        {
                            if (!dgv.Columns.Contains(prop.Name)) continue;

                            var cellValue = row.Cells[prop.Name].Value;
                            if (cellValue != null && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                            {
                                rowHasData = true;
                                break;
                            }
                        }

                        // Skip completely empty rows
                        if (!rowHasData) continue;

                        var model = new T();
                        foreach (var prop in properties)
                        {
                            if (!dgv.Columns.Contains(prop.Name)) continue;

                            var value = row.Cells[prop.Name].Value;
                            SetModelPropertyValue(model, prop, value);
                        }

                        models.Add(model);
                    }

                    return models;
                }

                // --- CASE 2: DataTable ---
                if (dataSource is DataTable dt)
                {
                    if (dt.Rows.Count == 0)
                        return models;

                    foreach (DataRow dr in dt.Rows)
                    {
                        var model = new T();
                        foreach (var prop in properties)
                        {
                            if (!dt.Columns.Contains(prop.Name)) continue;

                            var value = dr[prop.Name];
                            SetModelPropertyValue(model, prop, value);
                        }

                        models.Add(model);
                    }

                    return models;
                }

                return models;
            }

            // Helper method for safe conversion and assignment
            private static void SetModelPropertyValue<T>(T model, PropertyInfo prop, object value)
            {
                if (value == null || value == DBNull.Value) return;

                try
                {
                    object convertedValue = Convert.ChangeType(
                        value,
                        Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType
                    );
                    prop.SetValue(model, convertedValue);
                }
                catch
                {
                    // Intentionally ignored
                }
            }
        }

        // Model mapper for panels
        public static T BuildModelFromPanels<T>(Panel[] panels) where T : new()
        {
            var model = new T();
            var modelType = typeof(T);

            foreach (var prop in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Control control = null;

                foreach (var panel in panels)
                {
                    control = panel.Controls
                        .Cast<Control>()
                        .FirstOrDefault(c =>
                            c.Name.Equals("txt_" + prop.Name, StringComparison.OrdinalIgnoreCase) ||
                            c.Name.Equals("dtp_" + prop.Name, StringComparison.OrdinalIgnoreCase) ||
                            c.Name.Equals("cmb_" + prop.Name, StringComparison.OrdinalIgnoreCase));

                    if (control != null)
                        break;
                }

                if (control == null)
                    continue;

                object value = null;

                if (control is TextBox textBox)
                {
                    string tag = textBox.Tag?.ToString() ?? "";
                    bool isMoney = tag.IndexOf("MONEY", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool isDocument = tag.IndexOf("DOCUMENT", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (isMoney)
                    {
                        // MONEY: try exact stored value first
                        if (!string.IsNullOrWhiteSpace(textBox.AccessibleDescription) &&
                            decimal.TryParse(textBox.AccessibleDescription, out decimal exactVal))
                        {
                            value = exactVal;
                        }
                        else
                        {
                            // fallback parse formatted currency
                            if (decimal.TryParse(
                                textBox.Text,
                                NumberStyles.Currency,
                                CultureInfo.GetCultureInfo("en-PH"),
                                out decimal parsedDecimal))
                            {
                                value = parsedDecimal;
                            }
                            else
                            {
                                value = 0m;
                            }
                        }
                    }
                    else if (isDocument)
                    {
                        // DOCUMENT: get numeric value from AccessibleDescription
                        if (!string.IsNullOrWhiteSpace(textBox.AccessibleDescription) &&
                            int.TryParse(textBox.AccessibleDescription, out int docVal))
                        {
                            value = docVal;
                        }
                        else
                        {
                            // fallback: remove prefix and parse numeric part
                            string numericPart = new string(textBox.Text.Where(char.IsDigit).ToArray());
                            if (int.TryParse(numericPart, out int fallbackVal))
                                value = fallbackVal;
                            else
                                value = 0;
                        }
                    }
                    else
                    {
                        value = textBox.Text;
                    }
                }
                else if (control is ComboBox comboBox)
                {
                    if (comboBox.Tag?.ToString() == "DYNAMIC")
                        value = comboBox.SelectedValue;
                    else
                        value = comboBox.Text;
                }
                else if (control is DateTimePicker dateTimePicker)
                {
                    value = dateTimePicker.Value.ToString("MM/dd/yyyy");
                }

                if (value != null && prop.CanWrite)
                {
                    try
                    {
                        Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        object convertedValue = Convert.ChangeType(value, targetType);
                        prop.SetValue(model, convertedValue);
                    }
                    catch
                    {
                        // Optional: log error
                    }
                }
            }

            return model;
        }

        // Function to validate if a date follows MM/dd/yyyy format and is not earlier than today
        public static bool ValidateDateFormat(string dateText)
        {
            if (string.IsNullOrWhiteSpace(dateText))
            {
                Helpers.ShowDialogMessage("error", "Please enter a date.");
                return false;
            }

            if (DateTime.TryParseExact(
                dateText,
                "MM/dd/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime parsedDate))
            {
                // Check if the date is earlier than today
                if (parsedDate.Date < DateTime.Today)
                {
                    Helpers.ShowDialogMessage("error", "The date cannot be earlier than today.");
                    return false;
                }

                // Passed all checks
                return true;
            }
            else
            {
                // Invalid format
                Helpers.ShowDialogMessage("error", "Please ensure the date follows the format MM/dd/yyyy.");
                return false;
            }
        }

        //Back up all control values inside a panel (TextBox, ComboBox, CheckBox, DateTimePicker)
        public static Dictionary<string, string> BackupPanelData(Panel panel)
        {
            var backup = new Dictionary<string, string>();

            foreach (Control control in panel.Controls)
            {
                if (control is TextBox txt)
                    backup[txt.Name] = txt.Text;
                else if (control is ComboBox cmb)
                    backup[cmb.Name] = cmb.Text;
                else if (control is CheckBox chk)
                    backup[chk.Name] = chk.Checked.ToString();
                else if (control is DateTimePicker dtp)
                    backup[dtp.Name] = dtp.Value.ToString();
            }

            return backup;
        }

        //Restore control values inside a panel from a backup dictionary
        public static void RestorePanelData(Panel panel, Dictionary<string, string> backup)
        {
            if (backup == null) return;

            foreach (Control control in panel.Controls)
            {
                if (control is TextBox txt && backup.ContainsKey(txt.Name))
                {
                    txt.Text = backup[txt.Name];
                }
                else if (control is ComboBox cmb && backup.ContainsKey(cmb.Name))
                {
                    string savedValue = backup[cmb.Name];

                    // Fix: handle DropDownList style combos properly
                    if (cmb.DropDownStyle == ComboBoxStyle.DropDownList)
                    {
                        // If empty, clear selection
                        if (string.IsNullOrEmpty(savedValue))
                        {
                            cmb.SelectedIndex = -1;
                        }
                        else
                        {
                            // Try to select the saved item if it exists
                            int index = cmb.FindStringExact(savedValue);
                            cmb.SelectedIndex = index;
                        }
                    }
                    else
                    {
                        // For normal editable combos
                        cmb.Text = savedValue;
                    }
                }
                else if (control is CheckBox chk && backup.ContainsKey(chk.Name))
                {
                    chk.Checked = bool.TryParse(backup[chk.Name], out bool val) && val;
                }
                else if (control is DateTimePicker dtp && backup.ContainsKey(dtp.Name))
                {
                    if (DateTime.TryParse(backup[dtp.Name], out DateTime date))
                        dtp.Value = date;
                }
            }
        }

        public static async Task<bool> ValidateDataGridViewCells(DataGridView dgv, string[] columnsToCheck, bool showError = true)
        {
            bool hasError = false;
            List<DataGridViewCell> invalidCells = new List<DataGridViewCell>();

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                foreach (string colName in columnsToCheck)
                {
                    if (!dgv.Columns.Contains(colName))
                        continue;

                    var cell = row.Cells[colName];
                    string value = cell?.Value?.ToString()?.Trim();

                    bool isEmpty = string.IsNullOrEmpty(value);
                    bool isZero = false;

                    if (decimal.TryParse(value, out decimal numericValue))
                        isZero = numericValue == 0;

                    if (isEmpty || isZero)
                    {
                        hasError = true;
                        invalidCells.Add(cell);
                        cell.Style.BackColor = Color.Red;
                    }
                }
            }

            if (hasError)
            {
                if (showError)
                    ShowDialogMessage("error", "Please ensure all required fields are filled.");

                // Wait 3 seconds before resetting color
                await Task.Delay(3000);

                foreach (var cell in invalidCells)
                {
                    cell.Style.BackColor = Color.White;
                }
            }

            return hasError;
        }

        public static TextBox CreateSearchBox(string placeholderText, EventHandler onTextChanged)
        {
            TextBox txtSearch = new TextBox
            {
                Name = "txt_search",
                Dock = DockStyle.Top,
                ForeColor = Color.Gray,
                Text = placeholderText
            };

            // Event handlers
            txtSearch.Enter += (s, e) =>
            {
                if (txtSearch.Text == placeholderText)
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };

            txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtSearch.Text))
                {
                    txtSearch.Text = placeholderText;
                    txtSearch.ForeColor = Color.Gray;
                }
            };

            if (onTextChanged != null)
                txtSearch.TextChanged += onTextChanged;

            return txtSearch;
        }

        public static void SetButtonVisibility(ToolStrip toolStrip, IEnumerable<string> visibleButtons, IEnumerable<string> hiddenButtons)
        {
            if (toolStrip == null) return;

            // Make visible buttons visible
            foreach (var buttonName in visibleButtons ?? Enumerable.Empty<string>())
            {
                var btn = toolStrip.Items
                                   .OfType<ToolStripButton>()
                                   .FirstOrDefault(b => b.Name == buttonName);
                if (btn != null)
                    btn.Visible = true;
            }

            // Make hidden buttons invisible
            foreach (var buttonName in hiddenButtons ?? Enumerable.Empty<string>())
            {
                var btn = toolStrip.Items
                                   .OfType<ToolStripButton>()
                                   .FirstOrDefault(b => b.Name == buttonName);
                if (btn != null)
                    btn.Visible = false;
            }
        }

        public static void ResetControls(Panel[] pnls)
        {
            foreach (Panel pnl in pnls)
            {
                foreach (Control control in pnl.Controls)
                {
                    // Check if the control is a TextBox
                    if (control is TextBox textBox)
                    {
                        // Reset the TextBox's text
                        textBox.Text = "";
                    }
                    else if (control is ComboBox combobox)
                    {
                        combobox.SelectedIndex = -1;
                    }
                    // Reset DateTimePicker to current date
                    else if (control is DateTimePicker datePicker)
                    {
                        datePicker.Value = DateTime.Now;   // or DateTime.Today
                    }
                }
            }
        }

        public static void SetChildControlsEnabled(Control[] parents, bool enable, string[] excludeNames)
        {
            foreach (Control parent in parents)
            {
                foreach (Control control in parent.Controls)
                {
                    // Skip excluded controls
                    if (excludeNames != null && excludeNames.Contains(control.Name))
                        continue;

                    // Affect controls of these types
                    if (control is TextBox || control is ComboBox || control is CheckBox || control is DateTimePicker)
                        control.Enabled = enable;

                    // Recurse into child containers
                    if (control.HasChildren)
                        SetChildControlsEnabled(new Control[] { control }, enable, excludeNames);
                }
            }
        }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);

            // Get all properties of T
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static void EnableGroupHeaders(DataGridView dgv, Dictionary<string, string[]> columnGroups)
        {
            if (dgv == null || columnGroups == null || columnGroups.Count == 0)
                return;

            // Double buffer to reduce flickering
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, dgv, new object[] { true });

            // Redraw on scroll/resize
            dgv.Scroll += (s, e) => dgv.Invalidate();
            dgv.ColumnWidthChanged += (s, e) => dgv.Invalidate();

            // Paint group headers
            dgv.Paint += (s, e) => DrawGroupHeaders(dgv, e, columnGroups);

            // Override column header painting
            dgv.CellPainting += (s, e) => DrawGroupedHeaderCells(dgv, e);
        }

        private static void DrawGroupHeaders(DataGridView dgv, PaintEventArgs e, Dictionary<string, string[]> groups)
        {
            foreach (var group in groups)
            {
                string groupName = group.Key;
                string[] cols = group.Value;

                if (!cols.All(c => dgv.Columns.Contains(c)))
                    continue;

                DataGridViewColumn firstCol = dgv.Columns[cols.First()];
                DataGridViewColumn lastCol = dgv.Columns[cols.Last()];

                Rectangle r1 = dgv.GetCellDisplayRectangle(firstCol.Index, -1, true);
                Rectangle r2 = dgv.GetCellDisplayRectangle(lastCol.Index, -1, true);

                if (r1.IsEmpty || r2.IsEmpty) continue;

                Rectangle headerRect = new Rectangle(r1.X, r1.Y, r2.Right - r1.X, r1.Height / 2);

                using (Brush b = new SolidBrush(SystemColors.Control))
                    e.Graphics.FillRectangle(b, headerRect);

                e.Graphics.DrawRectangle(Pens.Gray, headerRect);

                TextRenderer.DrawText(e.Graphics, groupName,
                    dgv.ColumnHeadersDefaultCellStyle.Font,
                    headerRect, Color.Black,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private static void DrawGroupedHeaderCells(DataGridView dgv, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                e.PaintBackground(e.CellBounds, true);

                Rectangle fullRect = e.CellBounds;

                // Bottom half for column text
                Rectangle textRect = fullRect;
                textRect.Y += textRect.Height / 2;
                textRect.Height /= 2;

                TextRenderer.DrawText(e.Graphics,
                    e.FormattedValue?.ToString() ?? "",
                    e.CellStyle.Font, textRect,
                    e.CellStyle.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
        }

        public static class RandomNumber
        {
            private static Random random = new Random();

            // Generates a random 10-digit number as string
            public static string Generate10DigitNumber()
            {
                // First 5 digits
                int firstPart = random.Next(10000, 100000);
                // Last 5 digits
                int secondPart = random.Next(10000, 100000);

                return $"{firstPart}{secondPart}";
            }
        }

        /// <summary>
        /// Opens a file dialog to select a Word or PDF file.
        /// </summary>
        /// <returns>Full path of selected file, or null if cancelled.</returns>
        public static string PromptUploadFile()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select a Document";
                ofd.Filter = "Word Documents (*.docx)|*.docx|PDF Files (*.pdf)|*.pdf";
                ofd.Multiselect = false;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    return ofd.FileName;
                }
            }
            return null;
        }

        public static string ConvertFileToBase64(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(fileBytes);
        }

        /// <summary>
        /// Show loading overlay inside a DataGridView
        /// </summary>
        public static class Loading
        {
            private static UserControl overlayPanel;

            public static void ShowLoading(DataGridView dgv, string message = "Loading, please wait...")
            {
                if (overlayPanel != null) return; // already showing

                overlayPanel = new UserControl
                {
                    BackColor = Color.FromArgb(180, Color.Gray), // semi-transparent overlay
                    Dock = DockStyle.Fill
                };

                Label lblMessage = new Label
                {
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    Text = message,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                overlayPanel.Controls.Add(lblMessage);

                // Add overlay inside the DataGridView's parent (so it sits on top of the grid)
                dgv.Controls.Add(overlayPanel);
                overlayPanel.BringToFront();
            }

            /// <summary>
            /// Hide the loading overlay from the DataGridView
            /// </summary>
            public static void HideLoading(DataGridView dgv)
            {
                if (overlayPanel != null)
                {
                    dgv.Controls.Remove(overlayPanel);
                    overlayPanel.Dispose();
                    overlayPanel = null;
                }
            }
        }

        /// <summary>
        /// Updates a row in the DataGridView and sends the data to a service.
        /// </summary>
        /// <typeparam name="TModel">The type of the model being updated</typeparam>
        /// <param name="dgv">The DataGridView containing the row</param>
        /// <param name="rowIndex">The index of the row clicked</param>
        /// <param name="idColumnName">The name of the column that contains the unique ID</param>
        /// <param name="updateFunc">A function that sends the dictionary to a service and returns the updated model</param>
        public static async Task UpdateRowAsync<TModel>(
    DataGridView dgv,
    int rowIndex,
    string idColumnName,
    Func<string, Dictionary<string, object>, Task<(bool success, TModel data)>> updateFunc)
    where TModel : class, new()
        {
            try
            {
                if (rowIndex < 0) return; // Ignore header clicks

                var row = dgv.Rows[rowIndex];

                // Skip the new empty row
                if (row.IsNewRow) return;

                // Get the ID
                string id = row.Cells[idColumnName].Value?.ToString();

                // --- Continue building the data dictionary ---
                var data = new Dictionary<string, object>();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    string columnName = dgv.Columns[cell.ColumnIndex].Name;

                    if (cell.Value == null)
                    {
                        data[columnName] = null;
                        continue;
                    }

                    if (columnName.Equals("date", StringComparison.OrdinalIgnoreCase) ||
                        columnName.Equals("due", StringComparison.OrdinalIgnoreCase))
                    {
                        data[columnName] = DateHelper.ToIso8601(cell.Value);
                    }
                    else
                    {
                        data[columnName] = cell.Value;
                    }
                }

                var response = await updateFunc(id, data);

                if (!response.success || response.data == null) return;

                var updated = response.data;

                foreach (var prop in typeof(TModel).GetProperties())
                {
                    if (!dgv.Columns.Contains(prop.Name)) continue;

                    var value = prop.GetValue(updated);

                    if (prop.Name.Equals("date", StringComparison.OrdinalIgnoreCase) ||
                        prop.Name.Equals("due", StringComparison.OrdinalIgnoreCase))
                    {
                        row.Cells[prop.Name].Value = DateHelper.FormatDate(value);
                    }
                    else
                    {
                        row.Cells[prop.Name].Value = value;
                    }
                }

                Debug.WriteLine("Row updated successfully!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating row: {ex.Message}");
            }
        }


        public static class DateHelper
        {
            /// <summary>
            /// Converts an object to a formatted date string for display.
            /// Returns empty string if the input is null or cannot be parsed.
            /// </summary>
            /// <param name="value">The value to format</param>
            /// <param name="format">The desired date format (default: yyyy-MM-dd)</param>
            /// <returns>Formatted date string</returns>
            public static string FormatDate(object value, string format = "MM/dd/yyyy")
            {
                if (value == null) return string.Empty;

                if (value is DateTime dt)
                {
                    return dt.ToString(format);
                }

                if (DateTime.TryParse(value.ToString(), out dt))
                {
                    return dt.ToString(format);
                }

                return string.Empty;
            }


            /// <summary>
            /// Converts an object to ISO8601 string for backend (Go GORM)
            /// </summary>
            public static string ToIso8601(object value)
            {
                if (value == null) return null;

                DateTime dt;
                if (value is DateTime datetime)
                {
                    dt = datetime;
                }
                else if (!DateTime.TryParse(value.ToString(), out dt))
                {
                    return null;
                }

                return dt.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// Applies a search filter to a DataGridView bound to a BindingSource with DataTable.
        /// </summary>
        /// <param name="bindingSource">The BindingSource bound to the DataTable.</param>
        /// <param name="searchText">The text to search for.</param>
        /// <param name="columnsToSearch">The columns in the DataTable to include in the search.</param>
        public static void ApplySearchFilter(DataGridView dataGridView, string searchText, params string[] columnsToSearch)
        {
            if (dataGridView.DataSource == null)
                return;

            DataTable dt = null;

            // Check if DataSource is BindingSource -> unwrap to DataTable
            if (dataGridView.DataSource is BindingSource bs)
            {
                dt = bs.DataSource as DataTable;
                if (dt == null)
                    return;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    bs.RemoveFilter();
                    return;
                }

                string safeText = searchText.Replace("'", "''");

                var filters = columnsToSearch
                    .Where(col => dt.Columns.Contains(col))
                    .Select(col => $"CONVERT([{col}], System.String) LIKE '%{safeText}%'");

                string finalFilter = string.Join(" OR ", filters);

                bs.Filter = finalFilter;
            }
            // Check if DataSource is DataTable directly
            else if (dataGridView.DataSource is DataTable directDt)
            {
                dt = directDt;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    dt.DefaultView.RowFilter = string.Empty;
                    return;
                }

                string safeText = searchText.Replace("'", "''");

                var filters = columnsToSearch
                    .Where(col => dt.Columns.Contains(col))
                    .Select(col => $"CONVERT([{col}], System.String) LIKE '%{safeText}%'");

                string finalFilter = string.Join(" OR ", filters);

                dt.DefaultView.RowFilter = finalFilter;
            }
        }

        public static void ApplySearchFilterRecursive(DataGridView dataGridView, string searchText, params string[] columnsToSearch)
        {
            if (dataGridView.DataSource == null)
                return;

            DataTable dt = null;

            // Check if DataSource is BindingSource -> unwrap to DataTable
            if (dataGridView.DataSource is BindingSource bs)
            {
                dt = bs.DataSource as DataTable;
                if (dt == null)
                    return;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    bs.RemoveFilter();
                    return;
                }

                string safeText = searchText.Replace("'", "''");

                var filters = columnsToSearch
                    .Where(col => dt.Columns.Contains(col))
                    .Select(col => $"CONVERT([{col}], System.String) LIKE '%{safeText}%'");

                string finalFilter = string.Join(" OR ", filters);

                bs.Filter = finalFilter;
            }
            // Check if DataSource is DataTable directly
            else if (dataGridView.DataSource is DataView dv)
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    dv.RowFilter = string.Empty;
                    return;
                }

                string safeText = searchText.Replace("'", "''");

                var filters = dv.Table.Columns.Cast<DataColumn>()
                    .Where(col => columnsToSearch.Contains(col.ColumnName))
                    .Select(col => $"CONVERT([{col.ColumnName}], System.String) LIKE '%{safeText}%'");

                string finalFilter = string.Join(" OR ", filters);

                dv.RowFilter = finalFilter;
            }

        }

        /// <summary>
        /// Print text with optional print dialog.
        /// </summary>
        /// <param name="text">The text to print.</param>
        /// <param name="showDialog">Whether to show the print dialog before printing.</param>
        public static class Print
        {
            private static string _textToPrint = "";

            public static void PrintText(string text, bool showDialog = true)
            {
                if (string.IsNullOrEmpty(text))
                {
                    MessageBox.Show("Nothing to print.");
                    return;
                }

                _textToPrint = text;

                PrintDocument pd = new PrintDocument();
                pd.PrintPage += Pd_PrintPage;

                try
                {
                    if (showDialog)
                    {
                        using (PrintDialog printDialog = new PrintDialog())
                        {
                            printDialog.Document = pd;
                            if (printDialog.ShowDialog() == DialogResult.OK)
                            {
                                pd.Print();
                            }
                        }
                    }
                    else
                    {
                        // Directly print to default printer
                        pd.Print();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Printing error: " + ex.Message);
                }
            }

            private static void Pd_PrintPage(object sender, PrintPageEventArgs e)
            {
                e.Graphics.DrawString(
                    _textToPrint,
                    new Font("Arial", 14),
                    Brushes.Black,
                    100, 100
                );
            }
        }

        /// <summary>
        /// Set placeholder text (cue banner) in a TextBox.
        /// </summary>
        /// 
        public static class Placeholder
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

            private const int EM_SETCUEBANNER = 0x1501;

            public static void SetPlaceholder(TextBox textBox, string placeholder)
            {
                if (textBox == null) throw new ArgumentNullException(nameof(textBox));

                // If handle already exists, set immediately
                if (textBox.IsHandleCreated)
                {
                    SendMessage(textBox.Handle, EM_SETCUEBANNER, 0, placeholder);
                }
                else
                {
                    // If not, wait for handle creation
                    textBox.HandleCreated += (s, e) =>
                    {
                        SendMessage(textBox.Handle, EM_SETCUEBANNER, 0, placeholder);
                    };
                }
            }
        }

        /// <summary>
        /// Safely formats an object or DateTime into mm/dd/yyyy
        /// </summary>
        public static string FormatDate(object dateValue)
        {
            if (dateValue == null || dateValue == DBNull.Value)
                return string.Empty;

            if (DateTime.TryParse(dateValue.ToString(), out DateTime parsedDate))
            {
                return parsedDate.ToString("mm/dd/yyyy");
            }

            return string.Empty;
        }

        public static Dictionary<string, dynamic> GetControlsValues(Panel pnl)
        {

            Dictionary<string, dynamic> values = new Dictionary<string, dynamic>();

            foreach (Control control in pnl.Controls)
            {
                // Check if the control is a TextBox
                if (control is TextBox textBox)
                {
                    string key = textBox.Name.Replace("txt_", "");
                    string val = "";

                    if (textBox.Tag != null && textBox.Tag.Equals("MONEY"))
                    {

                        val = String.Format("{0}", textBox.Text.ToString().Replace(",", ""));
                    }
                    else
                    {
                        val = String.Format("{0}", textBox.Text.ToString());
                    }
                    values.Add(key, val);
                }

                // Check if the control is a Combobox
                if (control is ComboBox comboBox)
                {
                    string key = comboBox.Name.Replace("cmb_", "");
                    string val = "";



                    if (comboBox.Tag == "DYNAMIC")
                    {
                        key = key + "_id";
                        values.Add(key, comboBox.SelectedValue);
                    }

                    else
                    {
                        val = comboBox.Text.ToString();

                        values.Add(key, val);
                    }
                }

                // Check if the control is a Checkbox
                if (control is CheckBox checkbox)
                {
                    string key = checkbox.Name.Replace("chk_", "");
                    string val = String.Format("{0}", checkbox.Checked ? 1 : 0);
                    values.Add(key, val);
                }

                // Check if the control is a DATETIME PICKER
                if (control is DateTimePicker dateTimePicker)
                {
                    string key = dateTimePicker.Name.Replace("dtp_", "");
                    string val = String.Format("{0:yyyy-MM-dd}", dateTimePicker.Value);
                    values.Add(key, val);
                }

                // Check if the control is a NUMERIC
                if (control is NumericUpDown numericUpDown)
                {
                    string key = numericUpDown.Name.Replace("txt_", "");
                    string val = String.Format("{0}", numericUpDown.Value);
                    values.Add(key, val);
                }
            }

            return values;
        }

        public static Dictionary<string, dynamic> GetControlsValuesList(Panel[] panels)
        {
            Dictionary<string, dynamic> values = new Dictionary<string, dynamic>();

            foreach (Panel pnl in panels)
            {
                foreach (Control control in pnl.Controls)
                {
                    // Skip controls tagged as "EXCLUDED"
                    if (control.Tag != null && control.Tag.ToString().Equals("EXCLUDED", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // TextBox
                    if (control is TextBox textBox)
                    {
                        string key = textBox.Name.Replace("txt_", "");
                        string val = (textBox.Tag != null && textBox.Tag.Equals("MONEY"))
                            ? textBox.Text.Replace(",", "")
                            : textBox.Text;

                        values[key] = val;
                    }

                    // ComboBox
                    else if (control is ComboBox comboBox)
                    {
                        string key = comboBox.Name.Replace("cmb_", "");
                        string val = "";

                        if (comboBox.Tag != null && comboBox.Tag.ToString() == "DYNAMIC")
                        {
                            key += "_id";
                            values[key] = comboBox.SelectedValue;
                        }
                        else
                        {
                            val = comboBox.Text;
                            values[key] = val;
                        }
                    }

                    // CheckBox
                    else if (control is CheckBox checkbox)
                    {
                        string key = checkbox.Name.Replace("chk_", "");
                        string val = checkbox.Checked ? "1" : "0";
                        values[key] = val;
                    }

                    // DateTimePicker
                    else if (control is DateTimePicker dateTimePicker)
                    {
                        string key = dateTimePicker.Name.Replace("dtp_", "");
                        string val = dateTimePicker.Value.ToString("yyyy-MM-dd");
                        values[key] = val;
                    }

                    // NumericUpDown
                    else if (control is NumericUpDown numericUpDown)
                    {
                        string key = numericUpDown.Name.Replace("txt_", "");
                        string val = numericUpDown.Value.ToString();
                        values[key] = val;
                    }
                }
            }

            return values;
        }

        public static Dictionary<string, dynamic> GetControlsValues(Panel pnl1, Panel pnl2)
        {

            Dictionary<string, dynamic> values = new Dictionary<string, dynamic>();

            foreach (Control control in pnl1.Controls)
            {
                // Check if the control is a TextBox
                if (control is TextBox textBox)
                {
                    string key = textBox.Name.Replace("txt_", "");
                    string val = "";

                    if (textBox.Tag.ToString() == "MONEY")
                    {

                        val = String.Format("{0}", textBox.Text.ToString().Replace(",", ""));
                    }
                    else
                    {
                        val = String.Format("'{0}'", textBox.Text.ToString());
                    }
                    values.Add(key, val);
                }

                // Check if the control is a Combobox
                if (control is ComboBox comboBox)
                {
                    string key = comboBox.Name.Replace("cmb_", "");
                    string val = "";
                    if (string.IsNullOrEmpty(comboBox.Text))
                    {
                        val = "";
                    }
                    else
                    {
                        val = String.Format("'{0}'", comboBox.Text.ToString());
                    }
                    values.Add(key, val);
                }

                // Check if the control is a Checkbox
                if (control is CheckBox checkbox)
                {
                    string key = checkbox.Name.Replace("chk_", "");
                    string val = String.Format("{0}", checkbox.Checked ? 1 : 0);
                    values.Add(key, val);
                }

                // Check if the control is a DATETIME PICKER
                if (control is DateTimePicker dateTimePicker)
                {
                    string key = dateTimePicker.Name.Replace("dtp_", "");

                    string val = String.Format("'{0:yyyy-MM-dd}'", dateTimePicker.Value);

                    //string val = String.Format("'{0}'", dateTimePicker.Value);
                    values.Add(key, val);
                }

                // Check if the control is a NUMERIC
                if (control is NumericUpDown numericUpDown)
                {
                    string key = numericUpDown.Name.Replace("txt_", "");
                    string val = String.Format("'{0}'", numericUpDown.Value);
                    values.Add(key, val);
                }
            }

            foreach (Control control in pnl2.Controls)
            {
                // Check if the control is a TextBox
                if (control is TextBox textBox)
                {
                    string key = textBox.Name.Replace("txt_", "");
                    string val = "";

                    if (textBox.Tag.ToString() == "MONEY")
                    {

                        val = String.Format("'{0}'", textBox.Text.ToString().Replace(",", ""));
                    }
                    else
                    {
                        val = String.Format("'{0}'", textBox.Text.ToString().Replace(",", ""));
                    }
                    values.Add(key, val);
                }

                // Check if the control is a Combobox
                if (control is ComboBox comboBox)
                {
                    string key = comboBox.Name.Replace("cmb_", "");
                    string val = "";
                    if (string.IsNullOrEmpty(comboBox.Text))
                    {
                        val = "";
                    }
                    else
                    {
                        val = String.Format("'{0}'", comboBox.Text.ToString());
                    }
                    values.Add(key, val);
                }

                // Check if the control is a Checkbox
                if (control is CheckBox checkbox)
                {
                    string key = checkbox.Name.Replace("chk_", "");
                    string val = String.Format("{0}", checkbox.Checked ? 1 : 0);
                    values.Add(key, val);
                }

                // Check if the control is a DATETIME PICKER
                if (control is DateTimePicker dateTimePicker)
                {
                    string key = dateTimePicker.Name.Replace("dtp_", "");
                    string val = String.Format("'{0}'", dateTimePicker.Value);
                    values.Add(key, val);
                }

                // Check if the control is a NUMERIC
                if (control is NumericUpDown numericUpDown)
                {
                    string key = numericUpDown.Name.Replace("num_", "");
                    string val = String.Format("{0}", numericUpDown.Value);
                    values.Add(key, val);
                }

            }

            return values;
        }

        public static Dictionary<string, dynamic> GetControlsValues(Panel[] pnl1)
        {
            Dictionary<string, dynamic> values = new Dictionary<string, dynamic>();
            foreach (Panel pnl in pnl1)
            {
                foreach (Control control in pnl.Controls)
                {
                    // Check if the control is a TextBox
                    if (control is TextBox textBox)
                    {
                        string key = textBox.Name.Replace("txt_", "");
                        dynamic val = null;
                        if (textBox.Tag != null && textBox.Tag.ToString() == "MONEY")
                        {
                            if (decimal.TryParse(textBox.AccessibleDescription, out decimal exactVal))
                            {
                                val = exactVal;
                            }
                            else
                            {
                                // fallback to parsing cleaned text
                                string isParsed = GetCleanedPriceValue(textBox.Text);
                                if (decimal.TryParse(isParsed, out decimal tempVal))
                                {
                                    val = tempVal;
                                }
                                else
                                {
                                    MessageBox.Show("Invalid money format. Please enter a valid number.");
                                    val = 0.00;
                                }
                            }
                        }
                        else if (textBox.Tag != null && textBox.Tag is List<int> ids && ids.Count > 0)
                        {
                            // Assuming Tag contains a list of IDs (if applicable)
                            values.Add(key + "_id", ids);  // Add the list of IDs under the key + "_id"
                        }
                        else
                        {
                            val = textBox.Text.ToString();
                        }
                        values[key] = val;
                    }
                    if (control is ComboBox comboBox)
                    {
                        string key = comboBox.Name.Replace("cmb_", "");
                        string val = "";
                        if (comboBox.Tag == "DYNAMIC")
                        {
                            key = key + "_id";
                            values.Add(key, comboBox.SelectedValue);
                        }
                        else
                        {
                            val = comboBox.Text.ToString();
                            values.Add(key, val);
                        }
                    }
                    if (control is CheckBox checkbox)
                    {
                        string key = checkbox.Name.Replace("chk_", "");
                        string val = String.Format("{0}", checkbox.Checked ? 1 : 0);
                        values.Add(key, val);
                    }
                    if (control is DateTimePicker dateTimePicker)
                    {
                        string key = dateTimePicker.Name.Replace("dtp_", "");
                        string val = String.Format("{0:yyyy-MM-dd HH:mm:ss}", dateTimePicker.Value);
                        values.Add(key, val);
                    }
                    if (control is NumericUpDown numericUpDown)
                    {
                        string key = numericUpDown.Name.Replace("txt_", "");
                        string val = String.Format("'{0}'", numericUpDown.Value);
                        values.Add(key, val);
                    }
                }
            }
            return values;
        }

        public static bool ValidateControlsValues(Panel pnl)
        {
            bool isError = false;

            foreach (Control control in pnl.Controls)
            {
                string tag = control.Tag as string;
                if (string.IsNullOrEmpty(tag))
                    continue;

                bool isRequired = tag.IndexOf("REQUIRED", StringComparison.OrdinalIgnoreCase) >= 0;
                bool isMoney = tag.IndexOf("MONEY", StringComparison.OrdinalIgnoreCase) >= 0;

                if (control is TextBox textBox)
                {
                    string value = textBox.Text.Trim();

                    // REQUIRED validation
                    if (isRequired && string.IsNullOrEmpty(value))
                    {
                        FlashRed(textBox);
                        isError = true;
                        continue;
                    }

                    // MONEY validation
                    if (isMoney && !string.IsNullOrEmpty(value))
                    {
                        if (!decimal.TryParse(
                                value,
                                NumberStyles.Currency,
                                CultureInfo.GetCultureInfo("en-PH"),
                                out decimal moneyValue)
                            || moneyValue < 0)
                        {
                            FlashRed(textBox);
                            isError = true;
                            continue;
                        }
                    }

                    textBox.BackColor = SystemColors.Window;
                }
                else if (control is ComboBox comboBox)
                {
                    if (isRequired && comboBox.SelectedIndex < 0)
                    {
                        FlashRed(comboBox);
                        isError = true;
                    }
                    else
                    {
                        comboBox.BackColor = SystemColors.Window;
                    }
                }
                else if (control is DateTimePicker dtp)
                {
                    if (isRequired)
                    {
                        if (dtp.Value == dtp.MinDate || dtp.Value == default(DateTime))
                        {
                            FlashRed(dtp);
                            isError = true;
                        }
                        else
                        {
                            dtp.CalendarMonthBackground = SystemColors.Window;
                            dtp.BackColor = SystemColors.Window;
                        }
                    }
                }
            }

            return isError;
        }

        public static bool ValidateControlsValues(params Panel[] panels)
        {
            bool isError = false;

            foreach (var pnl in panels)
            {
                foreach (Control control in pnl.Controls)
                {
                    string tag = control.Tag as string;
                    if (string.IsNullOrEmpty(tag))
                        continue;

                    bool isRequired = tag.IndexOf("REQUIRED", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool isMoney = tag.IndexOf("MONEY", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (control is TextBox textBox)
                    {
                        string value = textBox.Text.Trim();

                        // REQUIRED validation
                        if (isRequired && string.IsNullOrEmpty(value))
                        {
                            FlashRed(textBox);
                            isError = true;
                            continue;
                        }

                        // MONEY validation
                        if (isMoney && !string.IsNullOrEmpty(value))
                        {
                            decimal moneyValue;

                            // Try exact stored value first (if you use AccessibleDescription)
                            if (!string.IsNullOrWhiteSpace(textBox.AccessibleDescription) &&
                                decimal.TryParse(textBox.AccessibleDescription, out moneyValue))
                            {
                                // valid
                            }
                            else if (!decimal.TryParse(
                                        value,
                                        NumberStyles.Currency,
                                        CultureInfo.GetCultureInfo("en-PH"),
                                        out moneyValue))
                            {
                                FlashRed(textBox);
                                isError = true;
                                continue;
                            }

                            if (moneyValue < 0)
                            {
                                FlashRed(textBox);
                                isError = true;
                                continue;
                            }
                        }

                        textBox.BackColor = SystemColors.Window;
                    }
                    else if (control is ComboBox comboBox)
                    {
                        if (isRequired && comboBox.SelectedIndex < 0)
                        {
                            FlashRed(comboBox);
                            isError = true;
                        }
                        else
                        {
                            comboBox.BackColor = SystemColors.Window;
                        }
                    }
                    else if (control is DateTimePicker dtp)
                    {
                        if (isRequired)
                        {
                            if (dtp.Value == dtp.MinDate || dtp.Value == default(DateTime))
                            {
                                FlashRed(dtp);
                                isError = true;
                            }
                            else
                            {
                                dtp.CalendarMonthBackground = SystemColors.Window;
                                dtp.BackColor = SystemColors.Window;
                            }
                        }
                    }
                }
            }

            return isError;
        }

        public static void FlashRed(Control control)
        {
            Color originalColor = SystemColors.Window;
            control.BackColor = Color.Red;

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 3000; // 3 seconds
            timer.Tick += (s, e) =>
            {
                control.BackColor = originalColor;
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        public static void BindControls(Panel[] pnl_list, DataTable dt, int selectedIndex = 0)
        {
            Dictionary<string, dynamic> values = new Dictionary<string, dynamic>();

            foreach (var col_name in dt.Columns)
            {
                foreach (var pnl in pnl_list)
                {
                    foreach (Control control in pnl.Controls)
                    {
                        if (control.Name.Contains(col_name.ToString()))
                        {
                            string column_name = col_name.ToString();
                            Console.WriteLine(column_name);

                            // Check if the control is a TextBox
                            if (control is TextBox textBox && textBox.Name.Replace("txt_", "") == column_name)
                            {
                                string key = textBox.Name.Replace("txt_", "");
                                object rawValue = dt.Rows[selectedIndex][column_name];

                                // MONEY FORMAT
                                if (textBox.Tag?.ToString().Contains("MONEY") == true)
                                {
                                    if (decimal.TryParse(rawValue.ToString(), out decimal moneyVal))
                                    {
                                        textBox.Text = moneyVal.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-PH"));
                                        textBox.AccessibleDescription = moneyVal.ToString(); // Store precise value
                                    }
                                    else
                                    {
                                        textBox.Text = "₱0.00";
                                        textBox.AccessibleDescription = "0";
                                    }
                                }

                                // DOCUMENT FORMAT
                                else if (textBox.Tag?.ToString().StartsWith("DOCUMENT") == true)
                                {
                                    string tag = textBox.Tag.ToString();   // e.g. "DOCUMENTAV REQUIRED"

                                    // Remove "DOCUMENT" and split by space, take first part
                                    string prefix = tag.Substring("DOCUMENT".Length).Split(' ')[0]; // "AV"

                                    if (int.TryParse(rawValue?.ToString(), out int docNumber))
                                    {
                                        textBox.Text = prefix + docNumber.ToString("D8");
                                        textBox.AccessibleDescription = docNumber.ToString(); // Store real value
                                    }
                                    else
                                    {
                                        textBox.Text = prefix + "00000000";
                                        textBox.AccessibleDescription = "0"; // fallback real value
                                    }
                                }

                                // MULTI TAG
                                else if (textBox.Tag is List<int> ids && ids.Count > 0)
                                {
                                    textBox.Text = string.Join(", ", ids);
                                }

                                // DEFAULT
                                else
                                {
                                    if (selectedIndex < 0 || selectedIndex >= dt.Rows.Count)
                                    {
                                        Console.WriteLine("IndexOutOfRangeException selectedIndex");
                                        return;
                                    }

                                    textBox.Text = rawValue?.ToString() ?? "";
                                }
                            }

                            // Check if the control is a Combobox
                            if (control is ComboBox comboBox)
                            {
                                Console.WriteLine($"This is a  combobox: {comboBox.Name} ");
                                string key = comboBox.Name.Replace("cmb_", "") + "_id";

                                if (comboBox.Tag == "DYNAMIC")
                                {
                                    Console.WriteLine("DYNAMICS:", comboBox.Name);
                                    comboBox.SelectedValue = (string)dt.Rows[selectedIndex][key].ToString();
                                }
                                // Check multiple values
                                else if (comboBox.Tag == "MULTIVALUE")
                                {
                                    string rawValue = dt.Rows[selectedIndex][column_name].ToString();
                                    var multiValues = rawValue.Split(',')
                                    .Select(v => v.Trim())
                                    .Where(v => !string.IsNullOrEmpty(v))
                                    .ToList();

                                    // Set the first value as the display text (optional behavior)
                                    comboBox.Text = multiValues.FirstOrDefault() ?? string.Empty;

                                    // Populate the ComboBox with all values
                                    //comboBox.Items.Clear();
                                    foreach (var val in multiValues)
                                    {
                                        comboBox.Items.Add(val);
                                    }

                                    // Optionally set the first item as selected (you could change this logic)
                                    if (multiValues.Count > 0)
                                    {
                                        comboBox.SelectedIndex = 0;  // Select the first item (if needed)
                                    }
                                }
                                else
                                {
                                    string keys = comboBox.Name.Replace("cmb_", "");
                                    comboBox.Text = (string)dt.Rows[selectedIndex][column_name].ToString();
                                }

                            }
                            // Check if the control is a Checkbox
                            if (control is CheckBox checkbox)
                            {
                                //to hand outofbound rows
                                if (selectedIndex < 0 || selectedIndex >= dt.Rows.Count)
                                {
                                    Console.WriteLine("IndexOutOfRangeException  ");
                                    return;
                                }
                                string key = checkbox.Name.Replace("chk_", "");
                                checkbox.Checked = (string)dt.Rows[selectedIndex][column_name].ToString() == "1" ||
                                (string)dt.Rows[selectedIndex][column_name].ToString().ToLower() == "true"
                                ? true : false;
                            }
                            // Check if the control is a DATETIME PICKER
                            if (control is DateTimePicker dateTimePicker)
                            {
                                if (selectedIndex < 0 || selectedIndex >= dt.Rows.Count)
                                    return;

                                object rawValue = dt.Rows[selectedIndex][column_name];

                                if (rawValue != DBNull.Value &&
                                    DateTime.TryParse(rawValue.ToString(), out DateTime parsedDate))
                                {
                                    dateTimePicker.Format = DateTimePickerFormat.Custom;
                                    dateTimePicker.CustomFormat = "MM/dd/yyyy";   // your format
                                    dateTimePicker.Value = parsedDate;
                                }
                                else
                                {
                                    // Make it appear empty
                                    dateTimePicker.Format = DateTimePickerFormat.Custom;
                                    dateTimePicker.CustomFormat = " ";
                                }
                            }
                            // Check if the control is a NUMERIC
                            if (control is NumericUpDown numericUpDown)
                            {
                                string key = numericUpDown.Name.Replace("txt_", "");
                                numericUpDown.Text = (string)dt.Rows[selectedIndex][column_name].ToString();
                            }
                        }
                    }
                }
            }
        }

        public static string GetLocalIPAddress()
        {
            string localIP = string.Empty;

            // Get the host name
            string hostName = Dns.GetHostName();

            // Get the list of IP addresses associated with the host
            foreach (var ip in Dns.GetHostAddresses(hostName))
            {
                // Check if it's an IPv4 address
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break; // Exit the loop after Getting the first IPv4 address
                }
            }

            return localIP;
        }
        public static string GetSerialNumber()
        {
            try
            {
                string serialNumber = string.Empty;
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");

                foreach (ManagementObject mo in searcher.Get())
                {
                    serialNumber = mo["SerialNumber"].ToString();
                    break; // Assuming only one motherboard
                }
                return serialNumber;
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error: " + ex.Message);
                return "";
            }
        }
        public static void ShowDialogMessage(string status, string message = "")
        {
            switch (status)
            {
                case "success":
                    MessageBox.Show(message, "SMPC SOFTWARE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "error":
                    MessageBox.Show(message, "SMPC SOFTWARE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                default:
                    // Handle unexpected status values
                    MessageBox.Show("Unknown status: " + status, "SMPC SOFTWARE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }
        public static void CopyFileTo(string filePath, string destinationPath)
        {
            try
            {
                File.Copy(filePath, destinationPath, true);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static DataTable ConvertDataGridViewToDataTable(DataGridView dgv)
        {
            DataTable dataTable = new DataTable();

            // Add columns to DataTable
            foreach (DataGridViewColumn column in dgv.Columns)
            {
                dataTable.Columns.Add(column.Name);
            }

            // Add rows to DataTable
            foreach (DataGridViewRow row in dgv.Rows)
            {
                // Skip the new row placeholder if it's present
                if (!row.IsNewRow)
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        dataRow[i] = row.Cells[i].Value;
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }
        public static string MoneyFormat(double money)
        {
            return String.Format("{0:N2}", money);
        }
        public static void GetModalData(TextBox textBox, DataView dataView)
        {
            int recordIndex = 0;
            textBox.Text = "";

            foreach (DataRowView rowView in dataView)
            {

                textBox.Text += recordIndex == 0 ? rowView["name"].ToString() : ", " + rowView["name"].ToString();
                recordIndex++;

            }

        }
        public static DataTable FilterDataTable(DataTable dataTable, string searchTerm, params string[] columnsToSearch)
        {
            if (dataTable == null || columnsToSearch == null || columnsToSearch.Length == 0)
            {
                return dataTable;
            }

            searchTerm = searchTerm?.ToLower() ?? string.Empty;

            var filteredRows = dataTable.AsEnumerable().Where(row =>
                columnsToSearch.Any(column =>
                    row[column]?.ToString().ToLower().Contains(searchTerm) == true));

            return filteredRows.Any() ? filteredRows.CopyToDataTable() : dataTable.Clone();
        }

        public static void GetBPIModalData(TextBox textBox, DataView dataView, int columnIndex)
        {
            if (dataView != null && dataView.Count > 0)
            {
                textBox.Text = dataView[0][columnIndex].ToString();
            }
        }
        public static void SetRowNumber(DataGridView grid, DataGridViewRowPostPaintEventArgs e, int columnIndex = 0)
        {
            if (grid != null && e.RowIndex >= 0 && columnIndex >= 0 && columnIndex < grid.ColumnCount)
            {
                grid.Rows[e.RowIndex].Cells[columnIndex].Value = (e.RowIndex + 1).ToString();
            }
        }
        public static void ClearDataGridView(DataGridView grid)
        {
            if (grid != null && grid.Rows.Count > 0)
            {
                grid.Rows.Clear();
            }
        }
        public static void LoadDirectory(string path, TreeView treeView)
        {
            // Clear any existing nodes
            treeView.Nodes.Clear();

            // Get the top-level directory and create a root node
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            TreeNode rootNode = new TreeNode(dirInfo.Name);
            treeView.Nodes.Add(rootNode);

            // Load subdirectories and files recursively
            LoadSubdirectoriesAndFiles(rootNode, dirInfo.FullName);
        }
        private static void LoadSubdirectoriesAndFiles(TreeNode parentNode, string path)
        {
            try
            {
                // Get all subdirectories in the given path
                string[] subdirectories = Directory.GetDirectories(path);

                foreach (string subdirectory in subdirectories)
                {
                    // Create a node for the subdirectory
                    DirectoryInfo dirInfo = new DirectoryInfo(subdirectory);
                    TreeNode subDirNode = new TreeNode(dirInfo.Name);

                    // Add the subdirectory node to the parent node
                    parentNode.Nodes.Add(subDirNode);

                    // Recursively load subdirectories and files into the current subdirectory node
                    LoadSubdirectoriesAndFiles(subDirNode, subdirectory);
                }

                // Get all files in the current directory and add them as leaf nodes
                string[] files = Directory.GetFiles(path);
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    TreeNode fileNode = new TreeNode(fileInfo.Name);
                    fileNode.Tag = file; // Store the full file path in the Tag property
                    parentNode.Nodes.Add(fileNode); // Add the file node
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle access permissions issues if necessary
            }
        }

        public static Dictionary<string, dynamic> MergeDictionaries(params Dictionary<string, dynamic>[] dictionaries)
        {
            var mergedDict = new Dictionary<string, dynamic>();

            foreach (var dict in dictionaries)
            {
                foreach (var kvp in dict)
                {
                    mergedDict[kvp.Key] = kvp.Value;
                }
            }
            return mergedDict;
        }

        public static Dictionary<string, dynamic> ConvertFieldsToDecimal(Dictionary<string, dynamic> data)
        {
            return data.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    if (kvp.Value == null)
                        return 0.00m;

                    try
                    {
                        return Convert.ToDecimal(kvp.Value);
                    }
                    catch
                    {
                        return kvp.Value; // Return original if conversion fails
                    }
                }
            );
        }

        public static bool ConvertFieldPropertiesType<T>(Dictionary<string, object> data, string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                MessageBox.Show("No keys provided for conversion.");
                return false;
            }

            bool success = true;

            foreach (var key in keys)
            {
                if (!data.ContainsKey(key))
                {
                    MessageBox.Show($"Missing field: {key.Replace("_", " ")}");
                    success = false;
                    continue;
                }

                if (data[key] is T correctTypeValue)
                {
                    data[key] = correctTypeValue; // already valid
                    continue;
                }

                if (!(data[key] is string value))
                {
                    MessageBox.Show($"Invalid value type for {key.Replace("_", " ")}. Expected a string.");
                    success = false;
                    continue;
                }

                value = value.Trim();

                if (string.IsNullOrEmpty(value))
                {
                    MessageBox.Show($"Empty value for {key.Replace("_", " ")}");
                    success = false;
                    continue;
                }

                object converted = null;

                if (typeof(T) == typeof(int) && int.TryParse(value, out var intVal))
                    converted = intVal;
                else if (typeof(T) == typeof(float) && float.TryParse(value, out var floatVal))
                    converted = floatVal;
                else if (typeof(T) == typeof(double) && double.TryParse(value, out var doubleVal))
                    converted = doubleVal;
                else if (typeof(T) == typeof(decimal) && decimal.TryParse(value, out var decVal))
                    converted = decVal;
                else if (typeof(T) == typeof(bool) && bool.TryParse(value, out var boolVal))
                    converted = boolVal;
                else
                {
                    MessageBox.Show($"Invalid format for {key.Replace("_", " ")}");
                    success = false;
                    continue;
                }

                data[key] = converted;
            }

            return success;
        }

        public static string GetCleanedPriceValue(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "0";
            // Remove currency symbols and thousands separators
            var cleaned = input.Replace("₱", "")
                               .Replace("$", "")
                               .Replace(",", "")
                               .Trim();
            return cleaned;
        }
        public static string FormatAsCurrency(TextBox textbox, decimal value, string currency = "PHP")
        {

            string currencyType = currency == "PHP" ? "en-PH" : "en-US";
            // Format and assign
            textbox.Text = value.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo(currencyType));
            textbox.Tag = "MONEY";
            textbox.AccessibleDescription = value.ToString();
            return textbox.Text;
        }

    }

}
