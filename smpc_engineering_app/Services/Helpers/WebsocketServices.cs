using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using smpc_engineering_app.Shared;

namespace smpc_engineering_app.Services.Helpers
{
    internal class WebSocketService : IDisposable
    {
        private ClientWebSocket _ws;
        private CancellationTokenSource _cts;
        private Func<string, Task> _callback;

        // The reconnect path has to remember the TYPE it was originally asked to deserialize
        // into. Holding it as a closure over the generic method is the only way to keep T
        // alive - see StartReconnect for what happened without it. Same approach the
        // inventory app's copy of this service already uses.
        private Func<Task> _reconnectAction;

        // One reconnect loop at a time. Every failure path calls StartReconnect (the
        // connect catch, the close branch, and the receive catch), so without this a single
        // dropped socket could spawn several loops, each retrying every 10 seconds and each
        // raising its own OnError - which the Layout turns into a MessageBox.
        private bool _isReconnecting;

        public string Endpoint { get; private set; }
        public bool IsConnected => _ws?.State == WebSocketState.Open;

        public event Action OnConnected;
        public event Action<string> OnError;
        public event Action OnDisconnected;
        static string wssUrl => Program.WssBaseUrl ?? "ws://127.0.0.1:3000/api/ws";
        public async Task ConnectAndDeserialize<T>(string endpoint, Action<T> onDeserialized)
        {
            string token = CacheData.SessionToken;


            string url = $"{wssUrl}{endpoint}?Authorization={token}";

            if (IsConnected) return;

            // Reconnecting replaces both of these, so let go of the previous pair rather than
            // leaving a dead socket and its token source behind on every retry.
            _ws?.Dispose();
            _cts?.Dispose();

            _ws = new ClientWebSocket();
            _cts = new CancellationTokenSource();
            Endpoint = endpoint;

            // Captures T and the caller's typed handler, so a reconnect re-enters this method
            // with the SAME type argument instead of inferring a new one.
            _reconnectAction = () => ConnectAndDeserialize<T>(endpoint, onDeserialized);

            _callback = (data) =>
            {
                try
                {
                    T result = JsonConvert.DeserializeObject<T>(data);
                    onDeserialized?.Invoke(result);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke("Deserialization failed: " + ex.Message);
                }
                return Task.CompletedTask;
            };

            try
            {
                await _ws.ConnectAsync(new Uri(url), _cts.Token);
                OnConnected?.Invoke();
                _ = ReceiveLoop(_cts.Token); // run in background
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Connection failed: " + ex.Message);
                StartReconnect();
            }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            var buffer = new byte[8192];
            var ms = new MemoryStream();

            try
            {
                while (_ws.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                        ms.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", token);
                        OnDisconnected?.Invoke();
                        StartReconnect();
                        break;
                    }

                    var message = Encoding.UTF8.GetString(ms.ToArray());
                    ms.SetLength(0);

                    if (message.Length > 1_000_000)
                    {
                        OnError?.Invoke("Message exceeds 1MB.");
                        continue;
                    }

                    if (_callback != null)
                        await _callback.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Receive error: " + ex.Message);
                OnDisconnected?.Invoke();
                StartReconnect();
            }
        }

        // Was:
        //     await ConnectAndDeserialize(Endpoint, async (string data) => await _callback(data));
        //
        // That handed the generic method an Action<string>, so T was inferred as STRING on
        // every reconnect. The callback it then built ran
        //     JsonConvert.DeserializeObject<string>(data)
        // against a JSON object, which fails with exactly
        //     "Unexpected character encountered while parsing value: {. Path '', line 1,
        //      position 1."
        // - the error reported on the engineering Sales Quotation screen (2026-09-05). So the
        // socket kept working until the first drop, and was permanently broken afterwards:
        // every message arriving after a reconnect raised OnError, which Layout.cs shows as
        // a MessageBox.
        //
        // It was also self-referential - the new _callback wrapped a lambda that re-read
        // _callback - so had the deserialize ever succeeded it would have recursed into
        // itself instead of reaching LoadQuotationRedBox.
        //
        // _reconnectAction keeps the original T, and is the same fix the inventory app's
        // copy of this service already carries.
        private void StartReconnect()
        {
            if (_isReconnecting) return;
            _isReconnecting = true;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(10000);

                    if (!IsConnected && _reconnectAction != null)
                        await _reconnectAction();
                }
                finally
                {
                    _isReconnecting = false;
                }
            });
        }

        public async Task SendAsync(string message)
        {
            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _ws.SendAsync(new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_ws != null)
            {
                try
                {
                    _cts?.Cancel();
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnected", CancellationToken.None);
                }
                catch { }
                finally
                {
                    _ws.Dispose();
                    _ws = null;
                }
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _ws?.Dispose();
        }
    }
}
