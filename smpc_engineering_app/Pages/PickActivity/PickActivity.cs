using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Services.Helpers;

namespace smpc_engineering_app.Pages.PickActivity
{
    public partial class PickActivity : UserControl
    {
        //Dictionaries for the column grouping of datagridviews
        Dictionary<string, string[]> columnGroupsMain = new Dictionary<string, string[]>()
        {
            { "- PICK LIST -", new string[] { "number", "item_code", "item_description", "left_qty", "left_uom", "pick_qty", "pick_uom" } },
            { "- PICK ACTIVITY -", new string[] { "actual_qty", "actual_uom", "bin_location" } },
        };

        public PickActivity()
        {
            InitializeComponent();

            Helpers.EnableGroupHeaders(dgv_main, columnGroupsMain);
        }
    }
}
