using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Helpers;

namespace smpc_engineering_app.Services.Transaction
{
    class JobOrderSOService : ServiceBase<JobOrderSOModel>
    {
        public JobOrderSOService() : base(ApiEndPoints.JOB_ORDER_SO) { }

        public async Task<DataTable> GetAsDatatable(string salesOrder)
        {
            try
            {
                var response = await ApiService<ApiResponseModel<List<JobOrderSOModel>>>.Get($"{ApiEndPoints.JOB_ORDER_SO}/{salesOrder}");

                DataTable data = JsonHelper.ToDataTable(response.data);

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataTable> GetAllAsDatatable()
        {
            try
            {
                var response = await ApiService<ApiResponseModel<List<JobOrderSOModel>>>.Get($"{ApiEndPoints.ALL_JOB_ORDER_SO}/0");

                //var response = await ApiService<ApiResponseModel<List<JobOrderSOModel>>>.Get($"{ApiEndPoints.ALL_JOB_ORDER_SO}/{CacheData.CurrentUser.employee_id}");

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
