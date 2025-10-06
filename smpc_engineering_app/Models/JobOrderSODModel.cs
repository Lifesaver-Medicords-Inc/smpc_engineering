using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace smpc_engineering_app.Models
{
    class JobOrderSODModel
    {
        [Column("item_code")]
        public string item_code { get; set; }

        [Column("item_desc")]
        public string item_desc { get; set; }

        [Column("stock")]
        public int stock { get; set; }

        [Column("req_qty")]
        public int req_qty { get; set; }

        [Column("remark")]
        public string remark { get; set; }

        [Column("status")]
        public string status { get; set; }
    }
}
