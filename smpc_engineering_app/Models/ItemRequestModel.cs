using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    public class ItemRequestModel
    {
        public int id { get; set; }
        public string doc_no { get; set; }
        public string req_dept { get; set; }
        public string purpose { get; set; }
        public string req_date { get; set; }
        public string required_date { get; set; }
        public string issue_date { get; set; }
        public string ref_doc { get; set; }
        public string req_by { get; set; }
        public string received_by { get; set; }
        public string approved_by { get; set; }
        public string issued_by { get; set; }
        public bool? is_forward { get; set; }
    }

    public class ItemRequestDetailsModel
    {
        public int id { get; set; }
        public int ir_id { get; set; }
        public int so_id { get; set; }
        public int sod_id { get; set; }
        public int item_id { get; set; }
        public string item_description { get; set; }
        public int order_qty { get; set; }
        public int req_qty { get; set; }
        public string req_uom { get; set; }
        public int issued_qty { get; set; }
        public int total_req { get; set; }
        public int total_issued { get; set; }
        public string issued_uom { get; set; }
        public string serial_no { get; set; }
        public string remarks { get; set; }
    }

    public class ItemRequestLocationModel
    {
        public int id { get; set; }
        public int ir_id { get; set; }
        public int ir_details_id { get; set; }
        public int issued_qty { get; set; }
        public int stock_qty { get; set; }
        public string issued_uom { get; set; }
        public string location { get; set; }
        public int warehouse_id { get; set; }
        public int item_id { get; set; }
    }

    public class ItemRequestList
    {
        public List<ItemRequestModel> item_request { get; set; }
        public List<ItemRequestDetailsModel> item_request_details { get; set; }
        public List<ItemRequestLocationModel> item_request_location { get; set; }
    }

    public class ItemRequestPayload
    {
        public ItemRequestModel item_request { get; set; }
        public List<ItemRequestDetailsModel> item_request_details { get; set; }
        public List<ItemRequestLocationModel> item_request_location { get; set; }
    }
}
