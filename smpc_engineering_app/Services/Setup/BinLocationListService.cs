using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Helpers;

namespace smpc_engineering_app.Services.Setup
{
    class BinLocationListService : ServiceBase<BinLocationModel>
    {
        public BinLocationListService() : base(ApiEndPoints.BIN_LOCATION) { }

        public async Task<DataTable> GetFilteredLocation(int itemId)
        {
            var response = await ApiService<ApiResponseModel<List<BinLocationModel>>>.Get($"{ApiEndPoints.BIN_LOCATION}/{itemId}");

            DataTable data = JsonHelper.ToDataTable(response.data);

            return data;
        }
    }
}
