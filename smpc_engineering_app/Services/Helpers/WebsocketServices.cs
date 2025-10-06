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

        public string Endpoint { get; private set; }
        public bool IsConnected => _ws?.State == WebSocketState.Open;

        public event Action OnConnected;
        public event Action<string> OnError;
        public event Action OnDisconnected;

        public async Task ConnectAndDeserialize<T>(string endpoint, Action<T> onDeserialized)
        {
            string token = CacheData.SessionToken;
            string url = $"ws://127.0.0.1:3000/api/ws{endpoint}?Authorization={token}";

            if (IsConnected) return;

            _ws = new ClientWebSocket();
            _cts = new CancellationTokenSource();
            Endpoint = endpoint;

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

        private void StartReconnect()
        {
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                if (!IsConnected && Endpoint != null)
                {
                    await ConnectAndDeserialize(Endpoint, async (string data) => await _callback(data));
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
