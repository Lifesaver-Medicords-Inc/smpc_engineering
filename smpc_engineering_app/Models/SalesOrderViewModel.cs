using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    class SalesOrderViewModel
    {
        public int so_id { get; set; }
        public int sod_id { get; set; }
        public string ref_doc { get; set; }
        public int item_id { get; set; }
        public string item_description { get; set; }
        public int order_qty { get; set; }
        public int req_qty { get; set; }
        public string req_uom { get; set; }
    }
}
