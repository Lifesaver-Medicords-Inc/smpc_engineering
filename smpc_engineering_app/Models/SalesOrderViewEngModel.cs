using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    public class SalesOrderViewEngModel
    {
        public int id { get; set; }
        public string customer { get; set; }
        public string tin { get; set; }
        public string code { get; set; }
        public string delivery_to { get; set; }
        public string bill_to { get; set; }
        public string doc_no { get; set; }
        public string date { get; set; }
        public string delivery_date { get; set; }
        public string reference_doc { get; set; }
        public string status { get; set; }
    }

    public class SalesOrderDetailsViewEngModel
    {
        public int id { get; set; }
        public int so_id { get; set; }
        public string item_code { get; set; }
        public string item_desc { get; set; }
        public int stock { get; set; }
        public int req_qty { get; set; }
        public string remark { get; set; }
        public string status { get; set; }
    }

    public class SalesOrderViewEngList
    {
        public List<SalesOrderViewEngModel> sales_order_view { get; set; }
        public List<SalesOrderDetailsViewEngModel> sales_order_details_view { get; set; }
    }
}
