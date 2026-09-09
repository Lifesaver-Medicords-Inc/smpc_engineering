using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace smpc_engineering_app.Pages.JobOrder
{
    // Per-unit serial number entry for one Job Order row.
    //
    // §5.23 / §10.7 say production generates serials automatically, one per unit. The
    // generation half is deliberately NOT built yet - no spec, §2.5 or otherwise, defines
    // what a production serial looks like, and inventing an identifier scheme is not
    // something to guess at (user decision 2026-09-05: "just leave it for now, maybe it can
    // type for every item"). So the serials are typed rather than generated - but they are
    // typed ONE PER UNIT, which is the part of §10.7 that was being ignored: the grid cell
    // used to take a single free-text blob for a row that may cover any quantity.
    //
    // When the generator does arrive it replaces what this modal writes; the storage shape
    // (one serial per unit, in order) does not change.
    public partial class SerialNumberEntryModal : Form
    {
        // §10.7's separator is not specified either. Comma-space reads naturally in the
        // grid cell, survives a round trip through nvarchar, and is what SplitSerials
        // parses back - keep the two in step if this ever changes.
        public const string Separator = ", ";

        public string SerialNumbers { get; private set; }

        private readonly int _quantity;

        public SerialNumberEntryModal(int quantity, string existing, string itemName)
        {
            InitializeComponent();

            // A job order with no quantity still gets one line - better than an empty modal
            // the user cannot type into at all.
            _quantity = quantity > 0 ? quantity : 1;

            Text = string.IsNullOrWhiteSpace(itemName) ? "Serial Numbers" : $"Serial Numbers - {itemName}";
            lbl_hint.Text =
                $"One serial number per unit ({_quantity} unit{(_quantity == 1 ? "" : "s")}). " +
                "Leave a line blank if that unit has no serial number.";

            var existingSerials = SplitSerials(existing);

            for (int unit = 0; unit < _quantity; unit++)
            {
                int rowIndex = dg_serials.Rows.Add();
                dg_serials.Rows[rowIndex].Cells["col_unit"].Value = $"Unit {unit + 1}";
                dg_serials.Rows[rowIndex].Cells["col_serial"].Value =
                    unit < existingSerials.Count ? existingSerials[unit] : string.Empty;
            }

            // More serials on record than units on the row - do not silently drop them.
            // Can happen if QTY was edited after the serials were entered.
            for (int extra = _quantity; extra < existingSerials.Count; extra++)
            {
                int rowIndex = dg_serials.Rows.Add();
                dg_serials.Rows[rowIndex].Cells["col_unit"].Value = $"Extra {extra - _quantity + 1}";
                dg_serials.Rows[rowIndex].Cells["col_serial"].Value = existingSerials[extra];
            }

            btn_ok.Click += btn_ok_Click;
            btn_cancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            CancelButton = btn_cancel;
        }

        public static List<string> SplitSerials(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return new List<string>();

            // Split on comma and on newline: a value typed into the old free-text cell may
            // carry either, and both should read back as a list rather than one long serial.
            return stored
                .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.None)
                .Select(s => s.Trim())
                .ToList();
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            dg_serials.EndEdit();

            var serials = new List<string>();
            foreach (DataGridViewRow row in dg_serials.Rows)
            {
                if (row.IsNewRow) continue;
                serials.Add(row.Cells["col_serial"].Value?.ToString()?.Trim() ?? string.Empty);
            }

            // Trailing blanks carry no information - drop them so a row where nothing was
            // entered saves as empty rather than as ", , , ". Blanks BETWEEN filled units
            // are kept, because position is what ties a serial to its unit.
            while (serials.Count > 0 && string.IsNullOrWhiteSpace(serials[serials.Count - 1]))
                serials.RemoveAt(serials.Count - 1);

            // §10.7 is prompt-and-proceed, applied generally: an incomplete serial column
            // asks, it never blocks. So a partially-filled set is a question, not an error,
            // and the user may always continue.
            int filled = serials.Count(s => !string.IsNullOrWhiteSpace(s));
            if (filled > 0 && filled < _quantity)
            {
                var proceed = MessageBox.Show(
                    $"Serial number column incomplete - {filled} of {_quantity} unit" +
                    $"{(_quantity == 1 ? "" : "s")} entered." + Environment.NewLine + Environment.NewLine +
                    "Proceed?",
                    "Serial Numbers",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);

                if (proceed != DialogResult.Yes) return;
            }

            SerialNumbers = string.Join(Separator, serials);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
