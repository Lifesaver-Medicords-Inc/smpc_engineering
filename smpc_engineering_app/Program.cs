using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;

namespace smpc_engineering_app
{
    // Public so a host process can point this assembly at the real API (see
    // Configure): Sales hosts this assembly in-process for the shared ItemSetUC
    // and the engineering sub-login at sign-in, and only ever references it,
    // so it must be visible across the assembly boundary.
    public static class Program
    {
        public static string ApiBaseUrl { get; private set; }
        public static string WssBaseUrl { get; private set; }

        // Called once by a host process at its own startup with its already-
        // resolved URLs. Without this, Main below never runs in the host, both
        // URLs stay null, and every engineering call - including the sales
        // login's engineering sub-login - falls back to the localhost
        // ApiService default (http://127.0.0.1:3000/api). Any PC without a
        // local API (e.g. a VPN-only machine) then fails sign-in with a bare
        // "Something went wrong". Blank values are ignored so a host that only
        // knows one URL cannot wipe the other.
        public static void Configure(string apiBaseUrl, string wssBaseUrl = null)
        {
            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
                ApiBaseUrl = apiBaseUrl;
            if (!string.IsNullOrWhiteSpace(wssBaseUrl))
                WssBaseUrl = wssBaseUrl;
        }

        // Ends this session on the server as the app closes. The API revokes a token on
        // /logout; no app ever called it, so a closed app's token stayed usable for the rest
        // of its 24 hours. Best effort - a slow or unreachable API must not hold up closing.
        private static void EndServerSession(string apiBaseUrl, string token)
        {
            if (string.IsNullOrWhiteSpace(apiBaseUrl) || string.IsNullOrWhiteSpace(token)) return;
            try
            {
                using (var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(3) })
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
                    // Task.Run keeps the blocking wait off the UI synchronization context.
                    Task.Run(() => client.PostAsync(apiBaseUrl.TrimEnd('/') + "/logout", null))
                        .Wait(TimeSpan.FromSeconds(3));
                }
            }
            catch
            {
                // Closing the app matters more than telling the server.
            }
        }
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
                // Engineering's pages from sales and inventory use this same token.
                EndServerSession(ApiBaseUrl, smpc_engineering_app.Shared.CacheData.SessionToken);

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
