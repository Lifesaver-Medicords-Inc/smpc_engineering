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
    class ItemRequestService2 : ServiceBase<ItemRequestList2>
    {
        public ItemRequestService2() : base(ApiEndPoints.ITEM_REQUEST2) { }

        // CREATE
        public async Task<ApiResponseModel<object>> CreateItemRequest(ItemRequestPayload2 payload)
        {
            var response = await ApiService<ApiResponseModel<object>>.Post(ApiEndPoints.ITEM_REQUEST2, new Dictionary<string, dynamic>
                {
                    { "item_request", payload.item_request },
                    { "item_request_details", payload.item_request_details },
                    { "item_request_locations", payload.item_request_locations }
                }
            );

            return response;
        }

        // UPDATE
        public async Task<ApiResponseModel<object>> UpdateItemRequest(ItemRequestPayload2 payload)
        {
            var response = await ApiService<ApiResponseModel<object>>.Put(ApiEndPoints.ITEM_REQUEST2, new Dictionary<string, dynamic>
                {
                    { "item_request", payload.item_request },
                    { "item_request_details", payload.item_request_details },
                    { "item_request_locations", payload.item_request_locations }
                }
            );

            return response;
        }

        // DELETE
        public async Task<ApiResponseModel<object>> DeleteItemRequest(ItemRequestPayload2 payload)
        {
            var response = await ApiService<ApiResponseModel<object>>.Delete(ApiEndPoints.ITEM_REQUEST2, new Dictionary<string, dynamic>
                {
                    { "item_request", payload.item_request },
                    { "item_request_details", payload.item_request_details },
                    { "item_request_locations", payload.item_request_locations }
                }
            );

            return response;
        }
    }
}
