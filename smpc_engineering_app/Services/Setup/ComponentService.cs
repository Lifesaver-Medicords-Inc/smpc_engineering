using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Helpers;
using System.Windows.Forms;
using System.Diagnostics;

namespace smpc_engineering_app.Services.Setup
{
    class ComponentService : ServiceBase<ComponentModel>
    {
        public ComponentService() : base(ApiEndPoints.COMPONENTS) { }

        public async Task<DataTable> GetAsDatatable(string bomId)
        {
            try
            {
                var response = await ApiService<ApiResponseModel<List<ComponentModel>>>.Get($"{ApiEndPoints.COMPONENTS}/{bomId}");

                if (response == null || response.data == null || response.data.Count == 0)
                {
                    return new DataTable(); // return empty table instead of crashing
                }


                DataTable data = JsonHelper.ToDataTable(response.data);

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
