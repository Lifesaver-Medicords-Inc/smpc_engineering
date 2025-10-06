using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace smpc_engineering_app.Models
{
    class JobOrderSOModel
    {
        [Column("id")]
        public int id { get; set; }

        [Column("customer")]
        public string customer { get; set; }

        [Column("tin")]
        public string tin { get; set; }

        [Column("code")]
        public string code { get; set; }

        [Column("delivery_to")]
        public string delivery_to { get; set; }

        [Column("bill_to")]
        public string bill_to { get; set; }

        [Column("doc_no")]
        public string doc_no { get; set; }

        [Column("date")]
        public string date { get; set; }

        [Column("delivery_date")]
        public string delivery_date { get; set; }

        [Column("reference_doc")]
        public int reference_doc { get; set; }

        [Column("status")]
        public string status { get; set; }
    }
}
