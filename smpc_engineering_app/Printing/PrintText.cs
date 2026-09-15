using System;
using System.Globalization;
using System.Windows.Forms;

namespace smpc_engineering_app.Printing
{
    // Small text helpers shared by this app's prints.
    internal static class PrintText
    {
        // The cell's value as text, or "" when the column is absent or the cell empty.
        // Numbers print without trailing zeros ("2", "2.5") and with thousands separators.
        public static string Cell(DataGridViewRow row, string column)
        {
            if (row?.DataGridView == null || !row.DataGridView.Columns.Contains(column))
                return "";

            object value = row.Cells[column].Value;
            if (value == null || value == DBNull.Value)
                return "";

            switch (value)
            {
                case decimal d: return d.ToString("#,##0.##", CultureInfo.InvariantCulture);
                case double f: return f.ToString("#,##0.##", CultureInfo.InvariantCulture);
                case float g: return g.ToString("#,##0.##", CultureInfo.InvariantCulture);
                case int i: return i.ToString("#,##0", CultureInfo.InvariantCulture);
                case long l: return l.ToString("#,##0", CultureInfo.InvariantCulture);
                default: return value.ToString().Trim();
            }
        }

        // A document number with its prefix (spec 2.5), whether the field holds the bare
        // number or the formatted one.
        public static string DocumentNo(string prefix, string text)
        {
            string value = (text ?? "").Trim();
            if (value.Length == 0)
                return "";
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return value;
            return smpc_engineering_app.Services.Helpers.Helpers.ComboBoxDocumentFormatter.FormatDocumentNo(prefix, value);
        }
    }
}
