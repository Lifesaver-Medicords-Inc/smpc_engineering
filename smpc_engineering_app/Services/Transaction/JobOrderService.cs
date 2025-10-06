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
    class JobOrderService : ServiceBase<JobOrderModel>
    {
        public JobOrderService() : base(ApiEndPoints.JOB_ORDER) { }

        public override async Task<DataTable> GetAsDatatable()
        {
            var response = await ApiService<ApiResponseModel<List<JobOrderModel>>>.Get($"{ApiEndPoints.JOB_ORDER}/0");

            // var response = await ApiService<ApiResponseModel<List<ProductionListModel>>>.Get($"{ApiEndPoints.PRODUCTION_LIST}/{CacheData.CurrentUser.employee_id}");

            DataTable data = JsonHelper.ToDataTable(response.data);

            return data;
        }

        public override async Task<DataTable> GetAsDatatable(Func<DataTable, DataTable> filter)
        {
            // Append "/0" when calling the API
            var response = await ApiService<ApiResponseModel<List<JobOrderModel>>>.Get($"{ApiEndPoints.JOB_ORDER}/0");

            DataTable data = JsonHelper.ToDataTable(response.data);

            return filter(data);
        }

        /// <summary>
        /// Saves the data: inserts if id=0, updates if id!=0
        /// </summary>
        public async Task<ApiResponseModel> Save(Dictionary<string, dynamic> data)
        {
            if (!data.ContainsKey("job_order_id"))
                throw new ArgumentException("Data dictionary must contain an 'job_order_id' key.");

            var job_order_id = Convert.ToInt32(data["job_order_id"]);

            Debug.WriteLine($"ID value before Save: {data["job_order_id"]} -> parsed as {job_order_id}");

            if (job_order_id == 0)
            {
                // ID = 0 → Insert
                return await Insert(data); 
            }
            else
            {
                // ID != 0 → Update
                var response = await Update(data);
                return new ApiResponseModel
                {
                    success = response?.success ?? false,
                    message = "Data has been updated"
                };
            }
        }
    }
}
