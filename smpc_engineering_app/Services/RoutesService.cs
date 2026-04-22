using smpc_engineering_app.Pages;
using smpc_engineering_app.Pages.Transactions;
using smpc_engineering_app.Pages.ItemRequest;
using smpc_engineering_app.Pages.ItemRequest2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Pages.PickActivity;
using smpc_engineering_app.Pages.PickActivity2;
using smpc_engineering_app.Pages.JobOrder;
using smpc_engineering_app.Pages.SalesOrderEngineering;

namespace smpc_engineering_app.Services
{
    class RoutesService
    {
        private Dictionary<string, Control> _pages = new Dictionary<string, Control>()
        {
            //========================================================================
            // TRANSACTIONS   
            { "Sales Quotation List", new SalesQuotationList() },
            { "Item Request", new ItemRequestPage2() },
            { "Pick Activity", new PickActivityPage2() },
            { "Job Order", new JobOrderPage() },
            { "Sales Order", new SalesOrderEngPage() },
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
