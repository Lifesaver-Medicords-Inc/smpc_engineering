using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Shared
{
     static class ENUM_TAX_CODE
    {
        public static DataTable LIST()
        {
            IEnumerable<string> data = new List<String>()
            {
                "VAT",
                "NON-VAT",
                "WC-140",
                "WC-160",
                "WC-515",
                "WV-010",
                "ZERO-RATED",
                "2306-VAT",
                "EWT",
                "IT",
            };
            DataTable table = new DataTable();
            table.Columns.Add("title");
            table.Columns.Add("value");

            foreach (string item in data)
            {

                DataRow dr = table.NewRow();
                dr["title"] = item;
                dr["value"] = item;

                table.Rows.Add(dr);
            }

            return table;

        }

        public static DataTable STATUS_LIST()
        {
            IEnumerable<string> data = new List<String>()
            {
                "INACTIVE",
                "ACTIVE",
                "FUTURE",    
            };
            DataTable table = new DataTable();
            table.Columns.Add("title");
            table.Columns.Add("value");

            foreach (string item in data)
            {

                DataRow dr = table.NewRow();
                dr["title"] = item;
                dr["value"] = item;

                table.Rows.Add(dr);
            }

            return table;

        }


    }
}
