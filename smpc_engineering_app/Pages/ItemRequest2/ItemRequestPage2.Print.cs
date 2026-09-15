using System;
using System.Linq;
using System.Windows.Forms;
using smpc_engineering_app.Printing;

namespace smpc_engineering_app.Pages.ItemRequest2
{
    public partial class ItemRequestPage2
    {
        // PRINT (spec 2.10, 5.9): the record on screen, on the house template. Read from the
        // page rather than re-fetched - the header fields and the grid are bound to the
        // record being viewed, exactly as the user sees it.
        private void btn_print_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_doc_no.Text))
            {
                MessageBox.Show("No Item Request selected.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var header = new ItemRequestPrint.Header
            {
                DocNo = PrintText.DocumentNo("IREQ#", txt_doc_no.Text),
                RequestDate = dtp_request_date.Text,
                RequiredDate = dtp_required_date.Text,
                IssueDate = dtp_issue_date.Text,
                RequestingDept = cmb_requesting_dept.Text,
                Purpose = txt_purpose.Text,
                ReferenceDoc = cmb_ref_doc.Text,
                RequestedBy = txt_requested_by.Text,
                ReceivedBy = cmb_received_by.Text,
                ApprovedBy = txt_approved_by.Text,
                IssuedBy = txt_issued_by.Text,
            };

            var lines = dgv_main.Rows.Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .Select(row => new ItemRequestPrint.Line
                {
                    Description = PrintText.Cell(row, "item_description"),
                    RequestQty = PrintText.Cell(row, "required_qty"),
                    RequestUom = PrintText.Cell(row, "required_uom"),
                    IssueQty = PrintText.Cell(row, "issued_qty"),
                    IssueUom = PrintText.Cell(row, "issued_uom"),
                    SerialNumbers = PrintText.Cell(row, "serial_no"),
                    Remarks = PrintText.Cell(row, "remarks"),
                })
                .Where(line => line.Description.Length > 0)
                .ToList();

            using (var preview = new smpc_inventory_app.Pages.Shared.PrintPreview(ItemRequestPrint.Build(header, lines)))
            {
                preview.ShowDialog(this);
            }
        }
    }
}
