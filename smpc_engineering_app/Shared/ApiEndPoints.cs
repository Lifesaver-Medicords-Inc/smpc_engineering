using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Shared
{
    public static class ApiEndPoints
    {
        //ENGINEERING
        public const string ENGINEERING = "/engineering";

        //ENGINEERING REDBOX
        public static string REDBOX = ENGINEERING + "/redboxlist";

        //QUOTATION LIST REDBOX WS
        public static string WSQUOTATIONREDBOXLIST = REDBOX + "/quotation";

        //JOB ORDER REDBOX WS
        public static string WSJOBORDERREDBOXLIST = REDBOX + "/job_order/0";

        //Sales Quotation List (§3.2 - REQUEST FOR ENGR. quotations scoped to the logged-in engineer)
        public const string SALES_QUOTATION_LIST = "/engineering/sales_quotation_list";

        //Sales Project (quotation detail editor - same endpoint smpc_sales_system's own
        //ProjectService uses for both GET and PUT)
        public const string SALES_PROJECTS = "/sales/projects";

        //Full item catalog (same endpoint smpc_sales_system's own ItemService uses -
        //All_ITEM above is a different, more limited view with no item_name/item_brand,
        //not enough for the Size Up pump picker's own filter+display needs)
        public const string SETUP_ITEM_FULL = "/setup/item";

        //Job Order
        public const string JOB_ORDER = "/engineering/job_order";
        public const string ENGINEER_LIST = "/engineering/job_order/engr_list";
        public const string COMPONENTS = "/engineering/job_order/components";

        //Sales Order Engineering
        public const string SALES_ORDER_ENGINEER = "/engineering/job_order/sales_order";

        //Item Request
        public const string ITEM_REQUEST2 = "/engineering/item_request";
        public const string ITEM_REQUEST2_ITEMS = "/engineering/item_request/items";
        public const string ITEM_REQUEST2_USERS = "/engineering/item_request/users";
        public const string ITEM_REQUEST2_SO_DOC = "/engineering/item_request/sales_order_doc";
        public const string ITEM_REQUEST2_SO = "/engineering/item_request/sales_order/";

        //Bin Location View
        public const string BIN_LOCATIONS = "/engineering/bin_location/";

        //Pick Activity
        public const string PICK_ACTIVITY2 = "/engineering/pick_activity";
        public const string PICK_ACTIVITY2_SO_DOC = "/engineering/pick_activity/sales_order_doc";
        public const string PICK_ACTIVITY2_SO = "/engineering/pick_activity/sales_order/";
        public static string PICK_ACTIVITY2_WAREHOUSE = "/engineering/pick_activity/warehouse";
        public static string PICK_ACTIVITY2_WAREHOUSE_AREA = "/engineering/pick_activity/warehouse_area/";

        //Clear Cache
        public const string CLEAR_CACHE = "/admin/clear_all";

        //Sales Order IR View
        public const string SALES_ORDER_IR_VIEW = "/setup/item/so_doc";

        //Sales Order PA View
        public const string SALES_ORDER_PA_VIEW = "/setup/pickAct/salesOrder";

        //All Item List
        public const string All_ITEM = "/setup/item/all_item";

        //Bin Location List
        public const string BIN_LOCATION = "/setup/item/all_binloc";

        //Warehouse Area List
        public const string WAREHOUSE_AREA = "/setup/pickAct/binloc";

        //All Item List
        public const string USER_LIST = "/setup/item/all_user";

        //Item Request
        public const string ITEM_REQUEST = "/setup/item/request";
        public const string ITEM_REQUEST_LOCATION = "/setup/item/request/location";

        //Pick Activity
        public const string PICK_ACTIVITY = "/setup/pickAct/list";

        //Engr. Users
        public const string ENGR_USERS = "/employee_users/Engineering";

    }
}