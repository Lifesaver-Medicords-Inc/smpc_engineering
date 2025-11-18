using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    class BinLocationModel
    {
        public int id { get; set; }
        public int ir_id { get; set; }
        public int ir_details_id { get; set; }
        public int pa_details_id { get; set; }
        public int issued_qty { get; set; }
        public string issued_uom { get; set; }
        public int actual_qty { get; set; }
        public string actual_uom { get; set; }
        public int stock_qty { get; set; }
        public string stock_uom { get; set; }
        public string location { get; set; }
        public int warehouse_id { get; set; }
        public int item_id { get; set; }
    }
}
