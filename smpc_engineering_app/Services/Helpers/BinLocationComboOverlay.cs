using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smpc_engineering_app.Models;
using System.Drawing;
using System.Windows.Forms;

namespace smpc_engineering_app.Services.Helpers
{
    /// <summary>
    /// Floats a single ComboBox over whichever bin_location cell is active in dgv_main.
    ///
    /// Cascade order (mirrors ReceivingWarehouseAreaView):
    ///   Stage 0 → zone
    ///   Stage 1 → area   (rows filtered by chosen zone)
    ///   Stage 2 → rack   (rows filtered by zone + area)
    ///   Stage 3 → level  (rows filtered by zone + area + rack)
    ///   Stage 4 → bins   (rows filtered by zone + area + rack + level)
    ///
    /// The assembled value written to bin_location is "zone-area-rack-level-bins".
    /// Each selection also updates the underlying BindingList model item so the
    /// payload built in btn_save_Click is always correct.
    ///
    /// Backspace strips the last "-segment", reverts to the previous stage list,
    /// and re-opens the dropdown automatically.
    ///
    /// The combo is invisible outside edit / new mode.
    /// </summary>
    public class BinLocationComboOverlay : IDisposable
    {
        // ── Injected dependencies ──────────────────────────────────────────────
        private readonly DataGridView _dgv;
        private readonly Func<int, List<PickActWarehouseAreaView>> _getAreaData;

        // ── Floating combo ─────────────────────────────────────────────────────
        private readonly ComboBox _combo;

        // ── Runtime state ──────────────────────────────────────────────────────
        private int _activeRow = -1;
        private bool _editing = false;
        private bool _suppress = false;   // blocks re-entrant SelectedIndexChanged

        // Stage definitions — property names in cascade order
        private static readonly string[] Stages = { "zone", "area", "rack", "level", "bins" };
        private const string SEP = "-";

        // ── Constructor ────────────────────────────────────────────────────────
        public BinLocationComboOverlay(
            DataGridView dgv,
            Func<int, List<PickActWarehouseAreaView>> getAreaData)
        {
            _dgv = dgv;
            _getAreaData = getAreaData;

            _combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDown,
                Visible = false,
                FlatStyle = FlatStyle.Flat,
                Font = dgv.Font,
                BackColor = SystemColors.Window,
                ForeColor = SystemColors.WindowText,
                IntegralHeight = true,
                MaxDropDownItems = 12,
            };

            // Combo events
            _combo.SelectedIndexChanged += OnComboSelected;
            _combo.KeyDown += OnComboKeyDown;
            _combo.KeyPress += OnComboKeyPress;
            _combo.Leave += OnComboLeave;

            // DGV events
            _dgv.CellClick += OnCellClick;
            _dgv.CellEnter += OnCellEnter;
            _dgv.Scroll += (s, e) => Reposition();
            _dgv.RowHeightChanged += (s, e) => Reposition();
            _dgv.Resize += (s, e) => Reposition();
            _dgv.ParentChanged += (s, e) => EnsureAttached();

            EnsureAttached();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Toggle visibility. Call from SetEditMode() in ReceivingReport2Page.
        /// Pass <c>true</c> when entering edit/new mode, <c>false</c> when leaving.
        /// </summary>
        public void SetEditingMode(bool enabled)
        {
            _editing = enabled;
            if (!enabled) HideCombo();
        }

        /// <summary>
        /// Call after _warehouseAreadata has been refreshed (end of LoadWarehouseArea)
        /// so the active row immediately picks up the new data.
        /// </summary>
        public void RefreshAreaData()
        {
            if (_editing && _activeRow >= 0)
                PopulateAndShow(_activeRow);
        }

        // ── Attach combo to a suitable host control ────────────────────────────

        private void EnsureAttached()
        {
            // Walk up the DGV's parent chain to find a Panel, Form, or UserControl
            Control host = _dgv.Parent;
            while (host != null &&
                   !(host is Panel || host is Form || host is UserControl))
                host = host.Parent;

            if (host != null && !host.Controls.Contains(_combo))
                host.Controls.Add(_combo);
        }

        // ── DGV event handlers ─────────────────────────────────────────────────

        private void OnCellClick(object sender, DataGridViewCellEventArgs e)
            => HandleActivation(e.ColumnIndex, e.RowIndex);

        private void OnCellEnter(object sender, DataGridViewCellEventArgs e)
            => HandleActivation(e.ColumnIndex, e.RowIndex);

        private void HandleActivation(int colIndex, int rowIndex)
        {
            int binCol = BinColIndex();
            if (!_editing || binCol < 0 || colIndex != binCol || rowIndex < 0)
            {
                HideCombo();
                return;
            }

            // Don't show overlay if the column is read-only
            var col = _dgv.Columns[binCol];
            if (col != null && col.ReadOnly)
            {
                HideCombo();
                return;
            }

            // ── NEW: don't show overlay if this row's has_actual is true ──
            if (IsRowHasActual(rowIndex))
            {
                HideCombo();
                return;
            }

            _activeRow = rowIndex;
            PopulateAndShow(rowIndex);
        }

        /// <summary>
        /// Returns true when the row's has_actual flag is explicitly set to true.
        /// </summary>
        private bool IsRowHasActual(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _dgv.Rows.Count) return false;

            DataGridViewColumn col = _dgv.Columns["has_actual"];
            if (col == null) return false;

            var cell = _dgv.Rows[rowIndex].Cells[col.Index];
            return cell?.Value != null
                && cell.Value != DBNull.Value
                && bool.TryParse(cell.Value.ToString(), out bool v)
                && v;
        }

        // ── Core: build item list, position combo, decide whether to auto-drop ─

        private void PopulateAndShow(int rowIndex)
        {
            int binCol = BinColIndex();
            if (binCol < 0 || rowIndex < 0 || rowIndex >= _dgv.Rows.Count) return;

            string current = CellValue(rowIndex);
            string[] parts = SplitValue(current);
            int stage = parts.Length;
            string[] items = BuildItems(parts, stage, rowIndex);

            _suppress = true;
            _combo.Items.Clear();
            foreach (var item in items)
                _combo.Items.Add(item);
            _combo.Text = current;          // show accumulated value
            _suppress = false;

            // Position precisely over the cell
            PositionOver(rowIndex, binCol);
            _combo.BringToFront();
            _combo.Visible = true;

            // Auto-drop only when there are choices left
            if (items.Length > 0 && stage < Stages.Length)
            {
                _combo.Focus();
                _combo.BeginInvoke(new Action(() =>
                {
                    if (_combo.Visible)
                        _combo.DroppedDown = true;
                }));
            }
        }

        private void PositionOver(int rowIndex, int colIndex)
        {
            // false = do NOT clip to the visible area; returns the full cell rect
            Rectangle cellRect = _dgv.GetCellDisplayRectangle(colIndex, rowIndex, false);
            Point screenPt = _dgv.PointToScreen(cellRect.Location);

            Control host = _combo.Parent;
            if (host == null) return;

            Point hostPt = host.PointToClient(screenPt);
            _combo.SetBounds(hostPt.X, hostPt.Y, cellRect.Width, cellRect.Height);
        }

        private void Reposition()
        {
            if (_combo.Visible && _activeRow >= 0)
            {
                int binCol = BinColIndex();
                if (binCol >= 0) PositionOver(_activeRow, binCol);
            }
        }

        private void HideCombo()
        {
            if (_combo.DroppedDown) _combo.DroppedDown = false;
            _combo.Visible = false;
            _activeRow = -1;
        }

        // ── Combo event handlers ───────────────────────────────────────────────

        private void OnComboSelected(object sender, EventArgs e)
        {
            if (_suppress || _activeRow < 0 || _combo.SelectedItem == null) return;

            int binCol = BinColIndex();
            if (binCol < 0) return;

            string current = CellValue(_activeRow);
            string chosen = _combo.SelectedItem.ToString();

            string newValue = string.IsNullOrEmpty(current)
                ? chosen
                : current + SEP + chosen;

            WriteCellValue(_activeRow, binCol, newValue);

            // Advance to the next stage — deferred so WinForms finishes its own
            // post-selection Text reset before we set the accumulated display value.
            _combo.BeginInvoke(new Action(() => PopulateAndShow(_activeRow)));
        }

        private void OnComboKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back && _activeRow >= 0)
            {
                e.SuppressKeyPress = true;
                RemoveLastSegment(_activeRow);
            }
        }

        // Block all typed characters — the combo is selection-only.
        // Backspace is already handled in KeyDown before KeyPress fires.
        private void OnComboKeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void OnComboLeave(object sender, EventArgs e)
        {
            // Keep the combo alive when focus stays within the DGV
            if (_dgv.ContainsFocus) return;
            HideCombo();
        }

        // ── Backspace: strip the last segment ─────────────────────────────────

        private void RemoveLastSegment(int rowIndex)
        {
            int binCol = BinColIndex();
            if (binCol < 0) return;

            string current = CellValue(rowIndex);
            if (string.IsNullOrEmpty(current)) return;

            int lastSep = current.LastIndexOf(SEP, StringComparison.Ordinal);
            string newValue = lastSep >= 0
                ? current.Substring(0, lastSep)
                : string.Empty;

            WriteCellValue(rowIndex, binCol, newValue);
            PopulateAndShow(rowIndex);      // re-open at the previous stage
        }

        // ── Cascading item builder ─────────────────────────────────────────────

        /// <summary>
        /// Filters _warehouseAreadata by all already-chosen segments, then
        /// returns distinct values for the next stage property.
        /// Returns an empty array if no data is loaded or all stages are complete.
        /// </summary>
        private string[] BuildItems(string[] chosenParts, int stage, int rowIndex)
        {
            if (stage >= Stages.Length) return Array.Empty<string>();

            var data = _getAreaData?.Invoke(rowIndex);
            if (data == null || data.Count == 0) return Array.Empty<string>();

            IEnumerable<PickActWarehouseAreaView> rows = data;
            for (int i = 0; i < stage; i++)
            {
                string chosen = chosenParts[i];
                string prop = Stages[i];
                rows = rows.Where(r =>
                    string.Equals(PropValue(r, prop), chosen, StringComparison.OrdinalIgnoreCase));
            }

            return rows
                .Select(r => PropValue(r, Stages[stage]))
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private static string PropValue(PickActWarehouseAreaView r, string prop)
        {
            if (prop == "zone") return r.zone ?? string.Empty;
            if (prop == "area") return r.area ?? string.Empty;
            if (prop == "rack") return r.rack ?? string.Empty;
            if (prop == "level") return r.level ?? string.Empty;
            if (prop == "bins") return r.bins ?? string.Empty;
            return string.Empty;
        }

        // ── Cell / model helpers ───────────────────────────────────────────────

        private int BinColIndex()
        {
            DataGridViewColumn col = _dgv.Columns["bin_location"];
            return col?.Index ?? -1;
        }

        private string CellValue(int rowIndex)
        {
            int binCol = BinColIndex();
            if (binCol < 0 || rowIndex < 0 || rowIndex >= _dgv.Rows.Count)
                return string.Empty;
            return _dgv.Rows[rowIndex].Cells[binCol].Value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Writes <paramref name="value"/> to the DataGridViewCell and also
        /// pushes it into the bound BindingList item via reflection so the
        /// model is always in sync before btn_save_Click reads it.
        /// </summary>
        private void WriteCellValue(int rowIndex, int colIndex, string value)
        {
            // 1. Cell
            _dgv.Rows[rowIndex].Cells[colIndex].Value = value;

            // 2. Bound model (BindingList<ReceivingReportDetailsModel>)
            if (_dgv.DataSource is System.ComponentModel.IBindingList bl
                && rowIndex < bl.Count)
            {
                object item = bl[rowIndex];
                System.Reflection.PropertyInfo prop =
                    item?.GetType().GetProperty("bin_location");
                prop?.SetValue(item, value);
            }
        }

        private static string[] SplitValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return Array.Empty<string>();
            return value.Split(new[] { SEP }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Immediately hides the overlay without changing editing mode.
        /// Call before reading grid data on save.
        /// </summary>
        public void Hide()
        {
            HideCombo();
        }

        // ── IDisposable ────────────────────────────────────────────────────────

        public void Dispose()
        {
            _combo.SelectedIndexChanged -= OnComboSelected;
            _combo.KeyDown -= OnComboKeyDown;
            _combo.KeyPress -= OnComboKeyPress;
            _combo.Leave -= OnComboLeave;
            _dgv.CellClick -= OnCellClick;
            _dgv.CellEnter -= OnCellEnter;

            _combo.Parent?.Controls.Remove(_combo);
            _combo.Dispose();
        }
    }
}
