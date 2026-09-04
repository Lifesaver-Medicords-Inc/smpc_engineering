using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    class QuotationRedboxListModel
    {
        public int id { get; set; }
        public string client_name { get; set; }
        public string sales_quotation { get; set; }
        public string status { get; set; }
        public string project_name { get; set; }
        public string sales_executive { get; set; }
        public string remark { get; set; }
        public int requested_engr_id { get; set; }
        // DATE REQUESTED column (grid col_date_requested binds this) + requester name.
        public string requested_for_engr_date { get; set; }
        public string requested_engr_name { get; set; }
    }

    class RedboxQuotationList
    {
        public List<QuotationRedboxListModel> quotationlist { get; set; }
    }
}
