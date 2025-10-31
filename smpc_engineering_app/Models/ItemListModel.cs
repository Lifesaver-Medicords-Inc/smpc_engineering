using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    class ItemListModel
    {
        public int item_id { get; set; }
        public string short_desc { get; set; }
        public string item_code { get; set; }
        public string general_name { get; set; }
        public string item_model { get; set; }
        public string uom_name { get; set; }
        public string size { get; set; }
    }
}
