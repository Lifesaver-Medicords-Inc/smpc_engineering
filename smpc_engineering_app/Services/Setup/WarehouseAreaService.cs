using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Helpers;

namespace smpc_engineering_app.Services.Setup
{
    class WarehouseAreaService : ServiceBase<WarehouseAreaModel>
    {
        public WarehouseAreaService() : base(ApiEndPoints.WAREHOUSE_AREA) { }
    }
}
