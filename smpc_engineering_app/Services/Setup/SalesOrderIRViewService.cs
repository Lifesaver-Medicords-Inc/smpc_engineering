using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;

namespace smpc_engineering_app.Services.Setup
{
    class SalesOrderIRViewService : ServiceBase<SalesOrderViewModel>
    {
        public SalesOrderIRViewService() : base(ApiEndPoints.SALES_ORDER_IR_VIEW) { }
    }
}
