using System.Collections.Generic;
using smpc_inventory_app.Printing.Core;

namespace smpc_engineering_app.Printing
{
    // The Pick List / Pick Activity (PA#) on the Class A house template (spec 2.10, 5.11).
    //
    // Two layouts, chosen with the Print Format selector at the foot of the page
    // (spec 2.10.4): the filled Pick Activity, and a blank Pick List whose pick-activity
    // columns are left empty so the warehouse can fill them by hand. Printed or not, the
    // Warehouse Manager still enters the actual picks and saves (spec 5.11).
    internal static class PickActivityPrint
    {
        public const string FormatPickActivity = "Pick Activity";
        public const string FormatBlankPickList = "Pick List (blank)";

        public class Header
        {
            public string DocNo { get; set; }
            public string ReferenceSo { get; set; }
            public string CustomerCode { get; set; }
            public string Customer { get; set; }
            public string SalesPerson { get; set; }
            public string PreparedBy { get; set; }
            public string PickedBy { get; set; }
        }

        public class Line
        {
            public string ItemCode { get; set; }
            public string Description { get; set; }
            public string QtyLeft { get; set; }
            public string LeftUom { get; set; }
            public string QtyToPick { get; set; }
            public string PickUom { get; set; }
            public string ActualQty { get; set; }
            public string ActualUom { get; set; }
            public string Warehouse { get; set; }
            public string NewBinLocation { get; set; }
        }

        public static HouseTemplateReport Build(Header header, IEnumerable<Line> lines, bool blank)
        {
            var report = new HouseTemplateReport { Title = blank ? "PICK LIST" : "PICK ACTIVITY" };

            string customer = string.IsNullOrWhiteSpace(header.CustomerCode)
                ? header.Customer
                : header.CustomerCode + " - " + header.Customer;
            report.LeftBlock.Add(HouseTemplateReport.Pair("CUSTOMER", customer));
            report.LeftBlock.Add(HouseTemplateReport.Pair("SALES PERSON", header.SalesPerson));

            report.RightBlock.Add(HouseTemplateReport.Pair("PA#", header.DocNo));
            report.RightBlock.Add(HouseTemplateReport.Pair("REFERENCE SO", header.ReferenceSo));

            report.Columns.Add(new HouseTemplateColumn("#", 'C'));
            report.Columns.Add(new HouseTemplateColumn("ITEM CODE"));
            report.Columns.Add(new HouseTemplateColumn("ITEM DESCRIPTION"));
            report.Columns.Add(new HouseTemplateColumn("QTY LEFT", 'R'));
            report.Columns.Add(new HouseTemplateColumn("UOM", 'C'));
            report.Columns.Add(new HouseTemplateColumn("QTY TO PICK", 'R'));
            report.Columns.Add(new HouseTemplateColumn("UOM", 'C'));
            report.Columns.Add(new HouseTemplateColumn("ACTUAL PICK QTY", 'R'));
            report.Columns.Add(new HouseTemplateColumn("UOM", 'C'));
            report.Columns.Add(new HouseTemplateColumn("NEW BIN LOCATION"));

            int number = 0;
            foreach (var line in lines)
            {
                number++;

                string bin = "";
                if (!blank)
                {
                    bin = string.IsNullOrWhiteSpace(line.Warehouse) || string.IsNullOrWhiteSpace(line.NewBinLocation)
                        ? (line.NewBinLocation ?? "")
                        : line.NewBinLocation + " (" + line.Warehouse + ")";
                }

                report.Rows.Add(new[]
                {
                    number.ToString(), line.ItemCode, line.Description,
                    line.QtyLeft, line.LeftUom, line.QtyToPick, line.PickUom,
                    blank ? "" : line.ActualQty, blank ? "" : line.ActualUom, bin,
                });
            }

            report.Signatures.Add(HouseTemplateReport.Pair("PREPARED BY", header.PreparedBy));
            report.Signatures.Add(HouseTemplateReport.Pair("PICKED BY", blank ? "" : header.PickedBy));

            return report;
        }
    }
}
