using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Helpers;

namespace smpc_engineering_app.Services.Transaction
{
    class SalesOrderViewEngService : ServiceBase<SalesOrderViewEngList>
    {
        public SalesOrderViewEngService() : base(ApiEndPoints.SALES_ORDER_ENGINEER) { }
    }
}
