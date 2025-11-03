using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;
using smpc_engineering_app.Services.Helpers;

namespace smpc_engineering_app.Services.Transaction
{
    class ItemRequestService : ServiceBase<ItemRequestList>
    {
        public ItemRequestService() : base(ApiEndPoints.ITEM_REQUEST) { }

        // CREATE
        public async Task<object> CreateIRRecord(ItemRequestPayload payload)
        {
            var response = await ApiService<ApiResponseModel<object>>.Post(ApiEndPoints.ITEM_REQUEST, new Dictionary<string, dynamic>
                {
                    { "item_request", payload.item_request },
                    { "item_request_details", payload.item_request_details },
                    { "item_request_location", payload.item_request_location }
                }
            );

            return response.data;
        }

        // UPDATE
        public async Task<object> UpdateIRRecord(ItemRequestPayload payload)
        {
            var response = await ApiService<ApiResponseModel<object>>.Put(ApiEndPoints.ITEM_REQUEST, new Dictionary<string, dynamic>
                {
                    { "item_request", payload.item_request },
                    { "item_request_details", payload.item_request_details },
                    { "item_request_location", payload.item_request_location }
                }
            );

            return response.data;
        }

        // DELETE
        public async Task<object> DeleteIRRecord(ItemRequestPayload payload)
        {
            var response = await ApiService<ApiResponseModel<object>>.Delete(ApiEndPoints.ITEM_REQUEST, new Dictionary<string, dynamic>
                {
                    { "item_request", payload.item_request },
                    { "item_request_details", payload.item_request_details },
                    { "item_request_location", payload.item_request_location }
                }
            );

            return response.data;
        }
    }
}
