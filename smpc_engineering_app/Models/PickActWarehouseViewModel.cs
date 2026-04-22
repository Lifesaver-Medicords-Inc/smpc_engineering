using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    public class PickActWarehouseViewModel
    {
        public int warehouse_id { get; set; }
        public string warehouse { get; set; }
        public string warehouse_address { get; set; }
    }

    public class PickActWarehouseAreaView
    {
        public int warehouse_area_id { get; set; }
        public string zone { get; set; }
        public string area { get; set; }
        public string rack { get; set; }
        public string level { get; set; }
        public string bins { get; set; }
    }
}
