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
    class PickActivityService : ServiceBase<PickActivityList>
    {
        public PickActivityService() : base(ApiEndPoints.PICK_ACTIVITY) { }

        // CREATE
        public async Task<object> CreatePARecord(PickActivityPayload payload)
        {
            var response = await ApiService<ApiResponseModel<object>>.Post(ApiEndPoints.PICK_ACTIVITY, new Dictionary<string, dynamic>
                {
                    { "pick_activity", payload.pick_activity },
                    { "pick_activity_details", payload.pick_activity_details },
                    { "pick_activity_location", payload.pick_activity_location }
                }
            );

            return response.data;
        }

        // UPDATE
        public async Task<object> UpdatePARecord(PickActivityPayload payload)
        {
            var response = await ApiService<ApiResponseModel<object>>.Put(ApiEndPoints.PICK_ACTIVITY, new Dictionary<string, dynamic>
                {
                    { "pick_activity", payload.pick_activity },
                    { "pick_activity_details", payload.pick_activity_details },
                    { "pick_activity_location", payload.pick_activity_location }
                }
            );

            return response.data;
        }

        // DELETE
        public async Task<object> DeletePARecord(PickActivityPayload payload)
        {
            var response = await ApiService<ApiResponseModel<object>>.Delete(ApiEndPoints.PICK_ACTIVITY, new Dictionary<string, dynamic>
                {
                    { "pick_activity", payload.pick_activity },
                    { "pick_activity_details", payload.pick_activity_details },
                    { "pick_activity_location", payload.pick_activity_location }
                }
            );

            return response.data;
        }
    }
}
