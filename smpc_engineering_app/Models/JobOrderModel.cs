using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    public class JobOrderModel
    {
        public int? id { get; set; }
        public int so_id { get; set; }
        public int ir_id { get; set; }
        public int bom_id { get; set; }
        public int order_details_id { get; set; }
        public string date { get; set; }
        public string sales_order { get; set; }
        public string type { get; set; }
        public string item_desc { get; set; }
        public int quantity { get; set; }
        public string materials { get; set; }
        public string due { get; set; }
        public int engr_id { get; set; }
        public string a_engr { get; set; }
        public string item_rqst { get; set; }
        public string status { get; set; }
        // §7.1 - the richer SO Item Status (Phase 2 item 2.6), joined from
        // tbl_trans_sales_order_details.status via order_details_id. Distinct from
        // `status` above, which is this Job Order's own coarse PENDING/ONGOING/COMPLETE
        // value (that's what drives which of the 3 tabs a row appears under).
        public string so_item_status { get; set; }
        public string general_name { get; set; }
        public string serial_no { get; set; }
        public string report { get; set; }
        public string report_base { get; set; }
    }
}
