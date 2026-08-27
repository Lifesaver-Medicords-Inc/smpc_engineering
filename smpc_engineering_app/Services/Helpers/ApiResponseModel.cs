using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Services.Helpers
{
    public class ApiResponseModel<T>
    {
        public bool success { get; set; }
        public T data { get; set; }
        // The server's own RespondError always sends {"success": false, "message": "..."}
        // (utils.RespondError in ERP_API), but this generic response shape had nowhere to
        // put it - every caller of a Post/Put/Delete typed to ApiResponseModel<T> could only
        // ever show a hardcoded, non-specific string on failure, never the server's actual
        // reason. Purely additive: existing deserialization just ignores the extra JSON
        // field when a caller doesn't read this property. (Same fix already made in the
        // Accounting app's copy of this class - Engineering was the one app missing it,
        // found while unifying all 6 apps' Login screens - Phase 4.6.)
        public string message { get; set; }
        public PaginationModel pagination { get; set; } = null;
    }

    public class ApiResponseModel
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class PaginationModel
    {
        public bool has_next { get; set; }
        public int page_size { get; set; }
    }

    public class PaginatedResult<T>
    {
        public T Data { get; set; }
        public PaginationModel Pagination { get; set; }
    }
}
