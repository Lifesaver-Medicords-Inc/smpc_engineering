using System.Collections.Generic;
using smpc_inventory_app.Printing.Core;

namespace smpc_engineering_app.Printing
{
    // The Item Request / Issuance (IREQ#) on the Class A house template (spec 2.10, 5.9).
    //
    // The layout is the Inventory app's HouseTemplate.rdlc - one shared house template,
    // copied into this app's output by the project file - and this class supplies only
    // the content, the same way Inventory's ReceivingReportPrint does.
    internal static class ItemRequestPrint
    {
        public class Header
        {
            public string DocNo { get; set; }
            public string RequestDate { get; set; }
            public string RequiredDate { get; set; }
            public string IssueDate { get; set; }
            public string RequestingDept { get; set; }
            public string Purpose { get; set; }
            public string ReferenceDoc { get; set; }
            public string RequestedBy { get; set; }
            public string ReceivedBy { get; set; }
            public string ApprovedBy { get; set; }
            public string IssuedBy { get; set; }
        }

        public class Line
        {
            public string Description { get; set; }
            public string RequestQty { get; set; }
            public string RequestUom { get; set; }
            public string IssueQty { get; set; }
            public string IssueUom { get; set; }
            public string SerialNumbers { get; set; }
            public string Remarks { get; set; }
        }

        public static HouseTemplateReport Build(Header header, IEnumerable<Line> lines)
        {
            var report = new HouseTemplateReport { Title = "ITEM REQUEST / ISSUANCE" };

            report.LeftBlock.Add(HouseTemplateReport.Pair("REQUESTING DEPT.", header.RequestingDept));
            report.LeftBlock.Add(HouseTemplateReport.Pair("PURPOSE", header.Purpose));

            report.RightBlock.Add(HouseTemplateReport.Pair("IREQ#", header.DocNo));
            report.RightBlock.Add(HouseTemplateReport.Pair("REQUEST DATE", header.RequestDate));
            report.RightBlock.Add(HouseTemplateReport.Pair("REQUIRED DATE", header.RequiredDate));
            report.RightBlock.Add(HouseTemplateReport.Pair("ISSUE DATE", header.IssueDate));
            report.RightBlock.Add(HouseTemplateReport.Pair("REFERENCE DOC.", header.ReferenceDoc));

            report.Columns.Add(new HouseTemplateColumn("#", 'C'));
            report.Columns.Add(new HouseTemplateColumn("ITEM DESCRIPTION"));
            report.Columns.Add(new HouseTemplateColumn("REQUEST QTY", 'R'));
            report.Columns.Add(new HouseTemplateColumn("UOM", 'C'));
            report.Columns.Add(new HouseTemplateColumn("ISSUE QTY", 'R'));
            report.Columns.Add(new HouseTemplateColumn("UOM", 'C'));
            report.Columns.Add(new HouseTemplateColumn("SERIAL NUMBER/S"));
            report.Columns.Add(new HouseTemplateColumn("REMARKS"));

            int number = 0;
            foreach (var line in lines)
            {
                number++;
                report.Rows.Add(new[]
                {
                    number.ToString(), line.Description, line.RequestQty, line.RequestUom,
                    line.IssueQty, line.IssueUom, line.SerialNumbers, line.Remarks,
                });
            }

            // The four signature blocks of spec 5.9. Names only until HRIS holds the
            // credentials those steps will authenticate against.
            report.Signatures.Add(HouseTemplateReport.Pair("REQUESTED BY", header.RequestedBy));
            report.Signatures.Add(HouseTemplateReport.Pair("RECEIVED BY", header.ReceivedBy));
            report.Signatures.Add(HouseTemplateReport.Pair("APPROVED-AUTHORIZED BY", header.ApprovedBy));
            report.Signatures.Add(HouseTemplateReport.Pair("ISSUED BY", header.IssuedBy));

            return report;
        }
    }
}
