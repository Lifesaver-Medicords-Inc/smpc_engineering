using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;

namespace smpc_engineering_app
{
    static class Program
    {
        public static string ApiBaseUrl { get; private set; }
        public static string WssBaseUrl { get; private set; }
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

            string env = System.Configuration.ConfigurationManager.AppSettings["Environment"] ?? "Production";

            // Resolve the correct API URL
            // smpc.endpoints.xml wins when present; App.config is the fallback.
            ApiBaseUrl = SmpcEndpoints.Api(
                System.Configuration.ConfigurationManager.AppSettings[$"ApiBaseUrl.{env}"])
                         ?? throw new ConfigurationErrorsException($"No API URL configured for environment: {env}");

            // Resolve the correct API URL
            WssBaseUrl = SmpcEndpoints.Ws(
                System.Configuration.ConfigurationManager.AppSettings[$"WssBaseUrl.{env}"])
                         ?? throw new ConfigurationErrorsException($"No API URL configured for environment: {env}");

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, args) =>
            {
                // Log the full stack (was only shown, never recorded) so the root cause of a
                // caught UI-thread error is still diagnosable from the log file.
                try { Log.Error(args.Exception, "Unhandled UI-thread exception"); } catch { }
                MessageBox.Show($"UI Thread Exception:\n\n{args.Exception.Message}",
                                "Unhandled UI Exception",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                try { Log.Error(ex, "Unhandled non-UI exception"); } catch { }
                MessageBox.Show($"Non-UI Exception:\n\n{ex?.Message}",
                                "Unhandled Exception",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            };

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
