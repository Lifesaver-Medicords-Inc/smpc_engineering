using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Shared
{
    public static class ApiEndPoints
    {

        public const string CHART_CLASS_SETUP = "/setup/chart_class";
        public const string CHART_GROUP_SETUP = "/setup/chart_group";
        public const string CHART_OF_ACCOUNT_SETUP = "/setup/chart_of_account";
        public const string CHART_OF_ACCOUNT_CLASSIFCATION_SETUP = "/setup/chart_of_account_classification/";
        public const string GENERAL_LEDGER_MAPPER_SETUP = "/setup/general_ledger";
        public const string BANK_SETUP = "/setup/bank";   
        public const string BOOK_SETUP = "/setup/book";
        public const string CURRENCY_SETUP = "/setup/currency";

        public const string PAYMENT_TERMS_SETUP = "/setup/payment_terms";

        //Tax Setup
        public const string TAX_SETUP = "/setup/tax";
        public const string TAX_CODE_SETUP = "/setup/tax_setup/";

        public const string EXPANDED_TAX_SETUP = "/setup/expanded_tax";
        public const string FINAL_TAX_SETUP = "/setup/final_tax";

        //Sales Invoice
        public const string SALES_ORDER_DR = "/sales/order_dr/";
        public const string SALES_INVOICE_DOC_NO = "/accounting/sales_invoice_doc_no";
        public const string SALES_INVOICE = "/accounting/sales_invoice";

        //ENGINEERING
        public const string ENGINEERING = "/engineering";

        //ENGINEERING REDBOX
        public static string REDBOX = ENGINEERING + "/redboxlist";

        //QUOTATION LIST REDBOX WS
        public static string WSQUOTATIONREDBOXLIST = REDBOX + "/quotation";

        //JOB ORDER REDBOX WS
        public static string WSJOBORDERREDBOXLIST = REDBOX + "/job_order/0";

        //Job Order Setup
        public const string JOB_ORDER = "/setup/job/order";
        public const string JOB_ORDER_SO = "/setup/job/sales";
        public const string ALL_JOB_ORDER_SO = "/setup/job/all_sales";
        public const string JOB_ORDER_SOD = "/setup/job/sales_details";
        public const string COMPONENTS = "/setup/job/components";

        //Sales Order IR View
        public const string SALES_ORDER_IR_VIEW = "/setup/item/so_doc";

        //Sales Order PA View
        public const string SALES_ORDER_PA_VIEW = "/setup/pickAct/salesOrder";

        //All Item List
        public const string All_ITEM = "/setup/item/all_item";

        //Sales Order Engineering
        public const string SALES_ORDER_ENGINEER = "/setup/job/so_view";

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
        public const string ENGR_USERS = "/employee_users/Admin";

    }
}