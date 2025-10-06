using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Helpers;

namespace smpc_engineering_app.Services.Transaction
{
    class JobOrderSODService : ServiceBase<JobOrderSODModel>
    {
        public JobOrderSODService() : base(ApiEndPoints.JOB_ORDER_SOD) { }

        public async Task<DataTable> GetAsDatatable(string orderId)
        {
            try
            {
                var response = await ApiService<ApiResponseModel<List<JobOrderSODModel>>>.Get($"{ApiEndPoints.JOB_ORDER_SOD}/{orderId}");

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
