using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;

namespace smpc_engineering_app
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration() 
                .MinimumLevel.Debug()
                .WriteTo.File("logs\\engineering-logs-.log", rollingInterval: RollingInterval.Day) 
                .CreateLogger();

            try
            { 
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new SMPC());

            }
            catch (Exception ex)
            {
                Log.Error("Exception Message: {Exception}", ex.Message); 
                Log.Error("Exception: {Exception}", ex.StackTrace);
                Log.Debug("=============================================");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
