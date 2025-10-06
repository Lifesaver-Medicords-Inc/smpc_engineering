using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Services.Helpers
{
    class LogsServices
    {
        public static void ErrorLogs(string errorMessage)
        {
            Console.WriteLine("ERROR : ", errorMessage); 
        }
    }
}
