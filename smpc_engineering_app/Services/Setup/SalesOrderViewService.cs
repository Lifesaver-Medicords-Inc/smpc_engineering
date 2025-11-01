using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;

namespace smpc_engineering_app.Services.Setup
{
    class SalesOrderViewService : ServiceBase<SalesOrderViewModel>
    {
        public SalesOrderViewService() : base(ApiEndPoints.SALES_ORDER_VIEW) { }
    }
}
