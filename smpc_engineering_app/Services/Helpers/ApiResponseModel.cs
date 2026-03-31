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
