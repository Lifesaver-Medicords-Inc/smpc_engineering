using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Services
{
    class GeneralService<T> : ServiceBase<T> where T : class
    {
        public GeneralService(string url) : base(url) { }
    }
}
