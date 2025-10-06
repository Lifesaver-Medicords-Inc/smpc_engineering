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

namespace smpc_engineering_app.Pages.Transactions
{
    public partial class SalesQuotationList : UserControl
    {
        public SalesQuotationList()
        {
            InitializeComponent();
            Helpers.Placeholder.SetPlaceholder(txt_search, "SEARCH BAR");
        }
    }
}
