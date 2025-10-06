using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    class JobOrderModel
    {

        [Column("job_order_id")]
        public int? job_order_id { get; set; }

        [Column("bom_id")]
        public int bom_id { get; set; }

        [Column("order_details_id")]
        public int order_details_id { get; set; }

        [Column("date")]
        public string date { get; set; }

        [Column("sales_order")]
        public string sales_order { get; set; }

        [Column("type")]
        public string type { get; set; }

        [Column("item_desc")]
        public string item_desc { get; set; }

        [Column("quantity")]
        public int quantity { get; set; }

        [Column("materials")]
        public string materials { get; set; }

        [Column("due")]
        public string due { get; set; }

        [Column("a_engr")]
        public string a_engr { get; set; }

        [Column("item_rqst")]
        public string item_rqst { get; set; }

        [Column("status")]
        public string status { get; set; }

        [Column("general_name")]
        public string general_name { get; set; }

        [Column("serial_no")]
        public string serial_no { get; set; }

        [Column("report")]
        public string report { get; set; }

        [Column("report_base")]
        public string report_base { get; set; }
    }
}
