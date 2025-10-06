using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Models
{
    public class CurrentUserModel
    {
        public int id { get; set; }
        public string employee_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string department { get; set; }
        public string position_id { get; set; }

        public UserPermissionModel permissions { get; set; }
        public PositionModel position { get; set; }
    }

    public class UserPermissionModel
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public bool can_create { get; set; }
        public bool can_update { get; set; }
        public bool can_delete { get; set; }
    }

    public class PositionModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public ICollection<PositionAccessModel> access { get; set; } = new List<PositionAccessModel>();
        public ICollection<CurrentUserModel> users { get; set; } = new List<CurrentUserModel>();
    }

    public class PositionAccessModel
    {
        public int id { get; set; }
        public int position_id { get; set; }
        public string code { get; set; }
    }
}
