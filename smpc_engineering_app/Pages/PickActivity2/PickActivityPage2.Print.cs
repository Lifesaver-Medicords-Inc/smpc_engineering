using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using smpc_engineering_app.Printing;

namespace smpc_engineering_app.Pages.PickActivity2
{
    public partial class PickActivityPage2
    {
        // The document has two print layouts, so a Print Format selector sits at the foot of
        // the page (spec 2.10.4). Added in code, once, when the page is created.
        private ComboBox _printFormat;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_printFormat != null)
                return;

            var bar = new Panel { Dock = DockStyle.Bottom, Height = 34, Padding = new Padding(8, 5, 8, 5) };

            _printFormat = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Left,
                Width = 170,
                Font = new Font("Microsoft Sans Serif", 9F),
            };
            _printFormat.Items.AddRange(new object[] { PickActivityPrint.FormatPickActivity, PickActivityPrint.FormatBlankPickList });
            _printFormat.SelectedIndex = 0;

            var label = new Label
            {
                Text = "Print Format",
                AutoSize = false,
                Dock = DockStyle.Left,
                Width = 90,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
            };

            // Added last so it docks first, taking the bottom edge before the grid fills the rest.
            bar.Controls.Add(_printFormat);
            bar.Controls.Add(label);
            Controls.Add(bar);
        }

        // PRINT (spec 2.10, 5.11): the record on screen, on the house template, in the chosen
        // format. Read from the page rather than re-fetched - the header fields and the grid are
        // bound to the record being viewed.
        private void btn_print_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_doc_no.Text))
            {
                MessageBox.Show("No Pick Activity selected.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool blank = string.Equals(_printFormat?.SelectedItem as string, PickActivityPrint.FormatBlankPickList);

            var header = new PickActivityPrint.Header
            {
                DocNo = PrintText.DocumentNo("PA#", txt_doc_no.Text),
                ReferenceSo = cmb_reference_so.Text.Length > 0 ? cmb_reference_so.Text : PrintText.DocumentNo("SO#", txt_reference_so.Text),
                CustomerCode = txt_customer_code.Text,
                Customer = txt_customer.Text,
                SalesPerson = txt_sales_person.Text,
                PreparedBy = txt_prepared_by.Text,
                PickedBy = txt_picked_by.Text,
            };

            var lines = dgv_main.Rows.Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .Select(row => new PickActivityPrint.Line
                {
                    ItemCode = PrintText.Cell(row, "item_code"),
                    Description = PrintText.Cell(row, "item_description"),
                    QtyLeft = PrintText.Cell(row, "left_qty"),
                    LeftUom = PrintText.Cell(row, "left_uom"),
                    QtyToPick = PrintText.Cell(row, "pick_qty"),
                    PickUom = PrintText.Cell(row, "pick_uom"),
                    ActualQty = PrintText.Cell(row, "actual_qty"),
                    ActualUom = PrintText.Cell(row, "actual_uom"),
                    Warehouse = PrintText.Cell(row, "warehouse"),
                    NewBinLocation = PrintText.Cell(row, "bin_location"),
                })
                .Where(line => line.Description.Length > 0 || line.ItemCode.Length > 0)
                .ToList();

            using (var preview = new smpc_inventory_app.Pages.Shared.PrintPreview(PickActivityPrint.Build(header, lines, blank)))
            {
                preview.ShowDialog(this);
            }
        }
    }
}
