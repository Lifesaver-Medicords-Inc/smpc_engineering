using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace smpc_engineering_app.Models
{
    class EngrUsersModel
    {
        [Column("first_name")]
        public string first_name { get; set; }

        [Column("last_name")]
        public string last_name { get; set; }
    }
}
