using System;
using System.Drawing;
using System.Windows.Forms;

namespace smpc_engineering_app.Services.Helpers
{
    /// <summary>
    /// Floats a single DateTimePicker over whichever cell of <paramref name="columnName"/>
    /// is active in a DataGridView - same "overlay control" pattern BinLocationComboOverlay
    /// uses for bin_location, since DataGridView has no built-in date-picker column type.
    ///
    /// Replaces free-typed date entry: previously a column like Job Order's DUE relied on
    /// the user typing an exact format, then a CellEndEdit handler tried to parse it and
    /// silently blanked the cell on failure - so a date the user *did* type could vanish
    /// without any visible error, only to surface later at Save time as a confusing
    /// "Due date is required". The picker only ever writes a valid date, formatted
    /// MM/dd/yyyy, so that failure mode can't happen anymore.
    ///
    /// The written value round-trips through the same MM/dd/yyyy format the caller's own
    /// validation and the API already expect - callers should format/parse against that.
    /// </summary>
    public class DateCellPickerOverlay : IDisposable
    {
        private const string DateFormat = "MM/dd/yyyy";

        // ── Injected dependencies ──────────────────────────────────────────────
        private readonly DataGridView _dgv;
        private readonly string _columnName;

        // ── Floating picker ─────────────────────────────────────────────────────
        private readonly DateTimePicker _picker;

        // ── Runtime state ──────────────────────────────────────────────────────
        private int _activeRow = -1;
        private bool _editing = false;
        private bool _suppress = false;   // blocks re-entrant ValueChanged while we seed it

        public DateCellPickerOverlay(DataGridView dgv, string columnName)
        {
            _dgv = dgv;
            _columnName = columnName;

            _picker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = DateFormat,
                Visible = false,
                Font = dgv.Font,
            };

            // Picker events
            _picker.ValueChanged += OnPickerValueChanged;
            _picker.Leave += OnPickerLeave;

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
        /// Toggle visibility. Call from SetEditMode() in the owning page.
        /// Pass <c>true</c> when entering edit/new mode, <c>false</c> when leaving.
        /// </summary>
        public void SetEditingMode(bool enabled)
        {
            _editing = enabled;
            if (!enabled) HidePicker();
        }

        /// <summary>
        /// Immediately hides the overlay without changing editing mode.
        /// Call before reading grid data on save.
        /// </summary>
        public void Hide()
        {
            HidePicker();
        }

        // ── Attach picker to a suitable host control ───────────────────────────

        private void EnsureAttached()
        {
            // Walk up the DGV's parent chain to find a Panel, Form, or UserControl
            // (TabPage derives from Panel, so a grid hosted directly on a tab page works too)
            Control host = _dgv.Parent;
            while (host != null &&
                   !(host is Panel || host is Form || host is UserControl))
                host = host.Parent;

            if (host != null && !host.Controls.Contains(_picker))
                host.Controls.Add(_picker);
        }

        // ── DGV event handlers ─────────────────────────────────────────────────

        private void OnCellClick(object sender, DataGridViewCellEventArgs e)
            => HandleActivation(e.ColumnIndex, e.RowIndex);

        private void OnCellEnter(object sender, DataGridViewCellEventArgs e)
            => HandleActivation(e.ColumnIndex, e.RowIndex);

        private void HandleActivation(int colIndex, int rowIndex)
        {
            int dueCol = TargetColIndex();
            if (!_editing || dueCol < 0 || colIndex != dueCol || rowIndex < 0)
            {
                HidePicker();
                return;
            }

            var col = _dgv.Columns[dueCol];
            if (col != null && col.ReadOnly)
            {
                HidePicker();
                return;
            }

            _activeRow = rowIndex;
            ShowPicker(rowIndex, dueCol);
        }

        // ── Core: seed the value, position the picker over the cell ────────────

        private void ShowPicker(int rowIndex, int colIndex)
        {
            string current = CellValue(rowIndex);

            _suppress = true;
            _picker.Value = DateTime.TryParseExact(current, DateFormat,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime parsed)
                ? parsed
                : DateTime.Today;
            _suppress = false;

            PositionOver(rowIndex, colIndex);
            _picker.BringToFront();
            _picker.Visible = true;
        }

        private void PositionOver(int rowIndex, int colIndex)
        {
            // false = do NOT clip to the visible area; returns the full cell rect
            Rectangle cellRect = _dgv.GetCellDisplayRectangle(colIndex, rowIndex, false);
            Point screenPt = _dgv.PointToScreen(cellRect.Location);

            Control host = _picker.Parent;
            if (host == null) return;

            Point hostPt = host.PointToClient(screenPt);
            _picker.SetBounds(hostPt.X, hostPt.Y, cellRect.Width, cellRect.Height);
        }

        private void Reposition()
        {
            if (_picker.Visible && _activeRow >= 0)
            {
                int dueCol = TargetColIndex();
                if (dueCol >= 0) PositionOver(_activeRow, dueCol);
            }
        }

        private void HidePicker()
        {
            _picker.Visible = false;
            _activeRow = -1;
        }

        // ── Picker event handlers ───────────────────────────────────────────────

        private void OnPickerValueChanged(object sender, EventArgs e)
        {
            if (_suppress || _activeRow < 0) return;

            int dueCol = TargetColIndex();
            if (dueCol < 0) return;

            WriteCellValue(_activeRow, dueCol, _picker.Value.ToString(DateFormat));
        }

        private void OnPickerLeave(object sender, EventArgs e)
        {
            // Keep the picker alive when focus stays within the DGV
            if (_dgv.ContainsFocus) return;
            HidePicker();
        }

        // ── Cell helpers ────────────────────────────────────────────────────────

        private int TargetColIndex()
        {
            DataGridViewColumn col = _dgv.Columns[_columnName];
            return col?.Index ?? -1;
        }

        private string CellValue(int rowIndex)
        {
            int dueCol = TargetColIndex();
            if (dueCol < 0 || rowIndex < 0 || rowIndex >= _dgv.Rows.Count)
                return string.Empty;
            return _dgv.Rows[rowIndex].Cells[dueCol].Value?.ToString() ?? string.Empty;
        }

        private void WriteCellValue(int rowIndex, int colIndex, string value)
        {
            // Data-bound cell: assigning Value pushes straight into the underlying
            // DataRow via DataPropertyName, same as JobOrderPage's own a_engr sync does.
            _dgv.Rows[rowIndex].Cells[colIndex].Value = value;
        }

        // ── IDisposable ────────────────────────────────────────────────────────

        public void Dispose()
        {
            _picker.ValueChanged -= OnPickerValueChanged;
            _picker.Leave -= OnPickerLeave;
            _dgv.CellClick -= OnCellClick;
            _dgv.CellEnter -= OnCellEnter;

            _picker.Parent?.Controls.Remove(_picker);
            _picker.Dispose();
        }
    }
}
