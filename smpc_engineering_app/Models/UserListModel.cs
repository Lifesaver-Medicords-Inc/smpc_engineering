using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    class UserListModel
    {
        public string user_name { get; set; }
    }

    class UserListViewModel
    {
        public int user_id { get; set; }
        public string user_name { get; set; }
    }
}
