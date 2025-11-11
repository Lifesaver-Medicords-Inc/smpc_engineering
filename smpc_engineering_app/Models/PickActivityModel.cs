using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    public class PickActivityModel
    {
        public int id { get; set; }
        public string doc_no { get; set; }
        public string customer { get; set; }
        public string code { get; set; }
        public string reference_so { get; set; }
        public string sales_person { get; set; }
        public string prepared_by { get; set; }
        public string picked_by { get; set; }
    }

    public class PickActivityDetailsModel
    {
        public int id { get; set; }
        public int pa_id { get; set; }
        public int item_id { get; set; }
        public string item_code { get; set; }
        public string item_description { get; set; }
        public int so_id { get; set; }
        public int sod_id { get; set; }
        public int left_qty { get; set; }
        public string left_uom { get; set; }
        public int pick_qty { get; set; }
        public string pick_uom { get; set; }
        public int actual_qty { get; set; }
        public string actual_uom { get; set; }
        public string bin_location { get; set; }
        public int order_qty { get; set; }
    }

    public class PickActivityLocationModel
    {
        public int id { get; set; }
        public int pa_id { get; set; }
        public int pa_details_id { get; set; }
        public int actual_qty { get; set; }
        public string actual_uom { get; set; }
        public string location { get; set; }
        public int warehouse_id { get; set; }
    }

    public class PickActivityList
    {
        public List<PickActivityModel> pick_activity { get; set; }
        public List<PickActivityDetailsModel> pick_activity_details { get; set; }
        public List<PickActivityLocationModel> pick_activity_location { get; set; }
    }

    public class PickActivityPayload
    {
        public PickActivityModel pick_activity { get; set; }
        public List<ItemRequestDetailsModel> pick_activity_details { get; set; }
        public List<ItemRequestLocationModel> pick_activity_location { get; set; }
    }
}
