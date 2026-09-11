using System;
using System.IO;
using System.Xml.Linq;

namespace smpc_engineering_app
{
    /// <summary>
    /// Resolves the API and WebSocket URLs from one external XML file, so
    /// repointing every SMPC app at a different backend is a text edit rather
    /// than six rebuilds.
    ///
    /// The problem it solves: a Cloudflare quick tunnel gets a new random
    /// hostname every time it restarts. That URL used to live in each app's
    /// App.config / appsettings.json, which meant editing and rebuilding all six
    /// clients - or hand-editing six .exe.config files in six output folders -
    /// every single time.
    ///
    /// Lookup order, first hit wins:
    ///   1. the path in the SMPC_ENDPOINTS environment variable, if set
    ///   2. smpc.endpoints.xml beside the running .exe   (per-app override)
    ///   3. %ProgramData%\SMPC\smpc.endpoints.xml        (shared by all apps)
    ///
    /// (2) exists so one app can be pointed somewhere else for testing without
    /// disturbing the other five. (3) is the normal case: one file, six apps.
    ///
    /// DEVELOPMENT OPTS OUT. When the app is configured for the Development
    /// environment the file is ignored completely and the app's own localhost
    /// settings are used. This file is a DEPLOYMENT mechanism - a developer who
    /// has explicitly selected Development means "talk to my local API", and
    /// silently redirecting them to whatever backend the server last published
    /// would make local debugging impossible to trust. To aim a dev build at
    /// the deployed backend on purpose, set Environment to Production.
    ///
    /// If no file is found, or it is malformed, the app's own built-in
    /// configuration is used exactly as before. This never throws and never
    /// blocks startup - a broken override must not be able to take down an app
    /// that was working, so a bad file is ignored and the reason recorded in
    /// Source.
    ///
    /// This class is duplicated verbatim in each of the six client repos. They
    /// are separate git repositories, so a shared linked file would break their
    /// independent builds. Keep the copies identical.
    /// </summary>
    internal static class SmpcEndpoints
    {
        public const string FileName = "smpc.endpoints.xml";

        /// <summary>Where the values came from. Log this at startup - it is the
        /// difference between "pointed at the wrong server" and "override file
        /// never loaded", which look identical from the UI.</summary>
        private static string _source = "built-in config (no override file)";
        /// Reading this triggers the file lookup if it has not happened yet -
        /// otherwise logging Source before the first Api()/Ws() call reports
        /// "no override file" even when one is about to load, which is the
        /// exact opposite of what this property is for.
        public static string Source { get { Load(); return _source; } }

        private static XElement _root;
        private static bool _loaded;

        /// <summary>API base URL, e.g. https://host/api. Returns fallback when
        /// no override file supplies one.</summary>
        public static string Api(string fallback)
        {
            string value = Read("api");

            if (string.IsNullOrWhiteSpace(value))
            {
                string baseUrl = Read("base");
                if (!string.IsNullOrWhiteSpace(baseUrl))
                    value = baseUrl.TrimEnd('/') + "/api";
            }

            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        /// <summary>WebSocket base URL, e.g. wss://host/api/ws. Derived from
        /// &lt;base&gt; unless given explicitly, so the common case is one line
        /// to edit rather than two that can disagree.</summary>
        public static string Ws(string fallback)
        {
            string value = Read("ws");

            if (string.IsNullOrWhiteSpace(value))
            {
                string baseUrl = Read("base");
                if (!string.IsNullOrWhiteSpace(baseUrl))
                    value = ToWebSocketScheme(baseUrl.TrimEnd('/')) + "/api/ws";
            }

            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        // https -> wss, http -> ws. Anything already using a ws scheme is left
        // alone, so an explicit <base>wss://...</base> still works.
        private static string ToWebSocketScheme(string url)
        {
            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return "wss://" + url.Substring("https://".Length);

            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                return "ws://" + url.Substring("http://".Length);

            return url;
        }

        private static string Read(string element)
        {
            Load();

            if (_root == null)
                return null;

            // Case-insensitive element match: this file is hand-edited, and
            // <API> failing silently while <api> works is a trap not worth
            // leaving in a file whose whole purpose is quick manual edits.
            foreach (XElement child in _root.Elements())
            {
                if (string.Equals(child.Name.LocalName, element, StringComparison.OrdinalIgnoreCase))
                {
                    string value = child.Value;
                    return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                }
            }

            return null;
        }

        // Mirrors the precedence each app's own Program.cs uses to pick its
        // environment - DOTNET_ENVIRONMENT first, then App.config's
        // "Environment" key - so this can never disagree with the environment
        // the rest of the app believes it is running in.
        private static bool IsDevelopment()
        {
            string env = null;

            try { env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"); }
            catch { }

            if (string.IsNullOrWhiteSpace(env))
            {
                try { env = System.Configuration.ConfigurationManager.AppSettings["Environment"]; }
                catch { }
            }

            return !string.IsNullOrWhiteSpace(env)
                && env.Trim().Equals("Development", StringComparison.OrdinalIgnoreCase);
        }

        private static void Load()
        {
            if (_loaded)
                return;

            _loaded = true;

            if (IsDevelopment())
            {
                _source = "built-in config (Development - override file ignored)";
                return;
            }

            foreach (string path in CandidatePaths())
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    continue;

                try
                {
                    _root = XDocument.Load(path).Root;
                    _source = path;
                }
                catch (Exception ex)
                {
                    // Malformed XML, locked file, permissions. Keep the built-in
                    // config rather than failing to start.
                    _root = null;
                    _source = "built-in config (" + path + " could not be read: " + ex.Message + ")";
                }

                return;
            }
        }

        private static System.Collections.Generic.IEnumerable<string> CandidatePaths()
        {
            string fromEnv = null;
            try { fromEnv = Environment.GetEnvironmentVariable("SMPC_ENDPOINTS"); }
            catch { }
            yield return fromEnv;

            string besideExe = null;
            try { besideExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName); }
            catch { }
            yield return besideExe;

            string shared = null;
            try
            {
                shared = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "SMPC", FileName);
            }
            catch { }
            yield return shared;
        }
    }
}
