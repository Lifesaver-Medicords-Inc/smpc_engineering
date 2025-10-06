using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace smpc_engineering_app.Models
{
    class ComponentModel
    {
        [Column("id")]
        public int id { get; set; }

        [Column("name")]
        public string name { get; set; }

        [Column("quantity")]
        public int quantity { get; set; }

        [Column("stock")]
        public int stock { get; set; }
    }
}
