using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using smpc_engineering_app.Shared;

namespace smpc_engineering_app.Services.Helpers
{

    public static class ApiService<T> where T : class
    {
        private static string baseUrl => Program.ApiBaseUrl ?? "http://127.0.0.1:3000/api";

        // ERP_API's RequireAuth middleware only ever delivers the auth token via a
        // Set-Cookie header on login - never in the JSON body - and expects it back
        // as a raw Authorization header (or ?Authorization= query param, for
        // WebSocketService's own use of CacheData.SessionToken) on every request
        // after. This class never captured it at all: a fresh HttpClient with no
        // CookieContainer was created per call, and CacheData.SessionToken (declared
        // in Shared/CacheData.cs) was never written to anywhere in this app - so
        // every "protected" endpoint call here has always been sent with no token,
        // and both red-box WebSockets fail with a 401 that ClientWebSocket reports
        // as the generic "Unable to connect to the remote server". Same missing-
        // token-capture gap already found and fixed in the Accounting app; mirrors
        // smpc_inventory_app's RequestToApi.cs, the confirmed-working reference.
        static private async Task<T> SendRequestAsync(string url, HttpMethod method, string body = null)
        {

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpContent content = null;
                    // If no content is provided, create an empty StringContent with Content-Type set to "application/json"
                    if (content == null && method != HttpMethod.Get)
                    {
                        content = new StringContent(body, Encoding.UTF8, "application/json");
                    }

                    // Create the HttpRequestMessage with the specified method (GET, POST, PUT, DELETE)
                    var requestMessage = new HttpRequestMessage(method, baseUrl + url)
                    {
                        Content = content
                    };

                    if (!string.IsNullOrEmpty(CacheData.SessionToken))
                    {
                        requestMessage.Headers.Add("Authorization", CacheData.SessionToken);
                    }

                    // Perform the HTTP request asynchronously
                    HttpResponseMessage response = await client.SendAsync(requestMessage);



                    // Check if the response is successful
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(CacheData.SessionToken) &&
                            response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
                        {
                            string token = ExtractToken(setCookieValues.First());
                            if (!string.IsNullOrEmpty(token))
                            {
                                CacheData.SessionToken = token;
                                MirrorTokenToSalesAssembly(token);
                            }
                        }

                        // Optionally, you can parse the responseContent into an object of type T
                        T result = JsonConvert.DeserializeObject<T>(responseContent);

                        // Display the response content (for debugging purposes)
                        //MessageBox.Show(responseContent, " Response");

                        return result; // Return the parsed result
                    }

                    else
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();


                        Debug.WriteLine(responseContent);
                        // Optionally, you can parse the responseContent into an object of type T
                        T result = JsonConvert.DeserializeObject<T>(responseContent);

                        // Display the response content (for debugging purposes)
                        //MessageBox.Show(responseContent, " Response");

                        return result; // Return the
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        // Same extraction logic as smpc_inventory_app's RequestToApi.cs - the cookie
        // looks like "Authorization=<jwt>; Path=/; Expires=...", so this pulls out
        // just the token value between "Authorization=" and the first semicolon (or
        // to the end of the string if there's no trailing attribute).
        // This app reuses ItemSetUC from the smpc_sales_system assembly, and that control
        // calls back into Sales's OWN service layer (RequestToApi), which reads Sales's
        // own smpc_sales_app.Data.CacheData.SessionToken - a completely separate static
        // from this app's CacheData.SessionToken. Inside the Engineering process that
        // Sales-side store was never populated, so every call ItemSetUC made went out with
        // no Authorization header and came back 401. That is why ASSIGNED ENGR. and
        // TEMPLATE were empty dropdowns and the stock indicators never resolved - the data
        // was fine, the requests were simply unauthenticated:
        //
        //   401 GET /api/engineering/job_order/engr_list      (ASSIGNED ENGR.)
        //   401 GET /api/setup/templates                      (TEMPLATE)
        //   401 GET /api/inventory/item_stocks/available      (INV. indicators)
        //   401 GET /api/inventory/item_stocks/reservations
        //
        // Mirroring the token across at capture keeps both stores in step. Wrapped because
        // it reaches into another assembly's static state - a failure here must not take
        // down the request that just succeeded.
        private static void MirrorTokenToSalesAssembly(string token)
        {
            try
            {
                smpc_sales_app.Data.CacheData.SessionToken = token;
            }
            catch
            {
                // Non-fatal: the Engineering app's own calls still work without it.
            }
        }

        private static string ExtractToken(string cookieString)
        {
            const string marker = "Authorization=";
            int tokenStartIndex = cookieString.IndexOf(marker);
            if (tokenStartIndex < 0) return null;
            tokenStartIndex += marker.Length;

            int tokenEndIndex = cookieString.IndexOf(";", tokenStartIndex);
            return tokenEndIndex == -1
                ? cookieString.Substring(tokenStartIndex)
                : cookieString.Substring(tokenStartIndex, tokenEndIndex - tokenStartIndex);
        }

        //// POST Method
        static internal async Task<T> Post(string url, HttpContent data)
        {
            string jsonContent = JsonConvert.SerializeObject(data);

            return await SendRequestAsync(url, HttpMethod.Post, jsonContent);
        }

        static internal async Task<T> Post(string url, object data)
        {
            string jsonContent = JsonConvert.SerializeObject(data);
            return await SendRequestAsync(url, HttpMethod.Post, jsonContent);
        }

        static internal async Task<T> Post(string url, Dictionary<string, dynamic> data)
        {
            string jsonContent = JsonConvert.SerializeObject(data);

            return await SendRequestAsync(url, HttpMethod.Post, jsonContent);
        }

        // PUT Method
        static internal async Task<T> Put(string url, HttpContent data)
        {
            string jsonContent = JsonConvert.SerializeObject(data);

            return await SendRequestAsync(url, HttpMethod.Put, jsonContent);
        }
        static internal async Task<T> Put(string url, Dictionary<string, object> data)
        {
            string jsonContent = JsonConvert.SerializeObject(data);

            return await SendRequestAsync(url, HttpMethod.Put, jsonContent);
        }

        static internal async Task<T> Put(string url, object data)
        {
            string jsonContent = JsonConvert.SerializeObject(data);
            return await SendRequestAsync(url, HttpMethod.Put, jsonContent);
        }

        // GET Method
        public static async Task<T> Get(string url)
        {
            return await SendRequestAsync(url, HttpMethod.Get);
        }

        //DELETE Method
        static internal async Task<T> Delete(string url, HttpContent data)
        {
            string jsonContent = JsonConvert.SerializeObject(data);

            return await SendRequestAsync(url, HttpMethod.Delete, jsonContent);
        }

        static internal async Task<T> Delete(string url, Dictionary<string, object> data)
        {
            string jsonContent = JsonConvert.SerializeObject(data);

            return await SendRequestAsync(url, HttpMethod.Delete, jsonContent);
        }
    }
}
