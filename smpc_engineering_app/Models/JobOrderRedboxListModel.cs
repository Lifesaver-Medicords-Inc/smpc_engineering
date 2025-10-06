using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    class JobOrderRedboxListModel
    {
        public int id { get; set; }
        public string client_name { get; set; }
        public string document_no { get; set; }
        public int items { get; set; }
        public string project_name { get; set; }
        public string due_date { get; set; }
        public string type { get; set; }
    }

    class RedboxJobOrder
    {
        public List<JobOrderRedboxListModel> joborder { get; set; }
    }
}
