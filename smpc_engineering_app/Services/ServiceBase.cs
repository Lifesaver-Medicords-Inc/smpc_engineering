using smpc_engineering_app.Services.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Services
{
    class ServiceBase<T> where T:class
    {
        private readonly string url ;

        public ServiceBase(string _url)
        {
            this.url = _url;
        }

        // GET
        public   virtual async Task<DataTable> GetAsDatatable()
        {
            try
            {
                var response = await ApiService<ApiResponseModel<List<T>>>.Get(url);

                DataTable data = JsonHelper.ToDataTable(response.data);

                return data;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public   virtual async Task<DataTable> GetAsDatatable(Func<DataTable, DataTable> filter)
        {
            var response = await ApiService<ApiResponseModel<List<T>>>.Get(url);

            DataTable data = JsonHelper.ToDataTable(response.data);

            return filter(data);
        }
          
        // POST
        public virtual async Task<ApiResponseModel> Insert(Dictionary<string, dynamic> data)
        {
            var response = await ApiService<ApiResponseModel>.Post(url, data);
            return response;
        }

        // DELETE
        public   async Task<bool> Delete(Dictionary<string, dynamic> data)
        {
            var response = await ApiService<ApiResponseModel<T>>.Delete(url, data);
            bool isSuccess = response.success;

            return isSuccess;
        }

        // UPDATE
        public virtual async Task<ApiResponseModel<T>> Update(Dictionary<string, dynamic> data)
        {
            var response = await ApiService<ApiResponseModel<T>>.Put(url, data);
            return response;
        }

        public virtual async Task<List<T>> GetList()
        {
            try
            {
                var response = await ApiService<ApiResponseModel<List<T>>>.Get(url);
                return response.data;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}