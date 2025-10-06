using smpc_engineering_app.Pages;
using smpc_engineering_app.Pages.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smpc_engineering_app.Services
{
    class RoutesService
    {
        private Dictionary<string, Control> _pages = new Dictionary<string, Control>()
        {
            //========================================================================
            // TRANSACTIONS   
            {"Job Orders", new JobOrder() },
            { "Sales Order", new SalesOrder() },
             
            //========================================================================
            // TRANSACTIONS 
            { "Sales Quotation List", new SalesQuotationList() },
        };

        private string _selectedRoute;
        public RoutesService(string selectedRoute)
        {
            this._selectedRoute = selectedRoute;
        }

        public Control GetForm()
        {
            return _pages.First(v => v.Key == this._selectedRoute).Value;
        }

        public String GetTitle()
        {
                return _pages.First(v => v.Key == this._selectedRoute).Key;
        }
    }
}
