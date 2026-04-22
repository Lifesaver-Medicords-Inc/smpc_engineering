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
        public string item_code { get; set; }
        public string customer { get; set; }
        public string code { get; set; }
        public string sales_person { get; set; }
        public string item_description { get; set; }
        public int order_qty { get; set; }
        public int req_qty { get; set; }
        public string req_uom { get; set; }
        public int left_qty { get; set; }
        public int pick_qty { get; set; }
        public string left_uom { get; set; }
    }

    class ItemRequestSalesOrderView
    {
        public int sales_order_details_id { get; set; }
        public int sales_order_id { get; set; }
        public int item_id { get; set; }
        public string item_desc { get; set; }
        public int required_qty { get; set; }
        public string required_uom { get; set; }
        public int remaining_qty { get; set; }
        public string remaining_uom { get; set; }
    }

    public class ItemRequestSalesOrderDocView
    {
        public int sales_order_id { get; set; }
        public string so_doc_no { get; set; }
    }

    public class PickActivitySalesOrderView
    {
        public int sales_order_id { get; set; }
        public string customer { get; set; }
        public string customer_code { get; set; }
        public string sales_person { get; set; }
    }

    public class PickActivitySalesOrderDetailsView
    {
        public int sales_order_details_id { get; set; }
        public int sales_order_id { get; set; }
        public int item_id { get; set; }
        public string item_code { get; set; }
        public string item_description { get; set; }
        public int pick_qty { get; set; }
        public string pick_uom { get; set; }
        public int left_qty { get; set; }
        public string left_uom { get; set; }
    }

    public class SalesOrderViewList
    {
        public List<PickActivitySalesOrderView> sales_order_view { get; set; }
        public List<PickActivitySalesOrderDetailsView> sales_order_details_view { get; set; }
    }

    public class PickActivitySalesOrderDocView
    {
        public int sales_order_id { get; set; }
        public string so_doc_no { get; set; }
    }
}
