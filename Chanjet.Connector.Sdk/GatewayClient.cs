using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chanjet.Connector.Sdk.Protocol;

namespace Chanjet.Connector.Sdk
{
    public class GatewayClient
    {
        private readonly string _appKey;
        private readonly string _appSecret;
        private readonly string _encryptKey;
        private readonly string _gatewayUrl;
        private readonly string _clientId;

        private ClientWebSocket? _webSocket;
        private MessageDispatcher? _dispatcher;
        private bool _running;
        private int _attempt = 0;
        private const int MaxBackoffSeconds = 60;

        private GatewayClient(string appKey, string appSecret, string? encryptKey, string? gatewayUrl)
        {
            _appKey = appKey;
            _appSecret = appSecret;
            _encryptKey = string.IsNullOrEmpty(encryptKey) ? appSecret : encryptKey;
            
            if (string.IsNullOrEmpty(gatewayUrl))
            {
                _gatewayUrl = "https://stream-open.chanapp.chanjet.com";
            }
            else
            {
                _gatewayUrl = gatewayUrl;
            }

            string hostname = Environment.MachineName;
            int pid = Process.GetCurrentProcess().Id;
            string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            _clientId = $"{_appKey}@{hostname}_{pid}_{uniqueId}";
        }

        public void UseDispatcher(MessageDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_dispatcher == null)
            {
                throw new InvalidOperationException("MessageDispatcher is not configured. Call UseDispatcher first.");
            }

            _running = true;

            while (_running && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ConnectAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GatewayClient] Connection error: {ex.Message}");
                }

                if (!_running || cancellationToken.IsCancellationRequested) break;

                _attempt++;
                int backoff = Math.Min((int)Math.Pow(2, _attempt), MaxBackoffSeconds);
                Console.WriteLine($"[GatewayClient] Reconnecting in {backoff} seconds (attempt {_attempt})...");
                await Task.Delay(TimeSpan.FromSeconds(backoff), cancellationToken);
            }
        }

        public void Stop()
        {
            _running = false;
            _webSocket?.Abort();
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            using (_webSocket = new ClientWebSocket())
            {
                string wsUrl = _gatewayUrl.Replace("http://", "ws://").Replace("https://", "wss://");
                
                string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                string nonce = Guid.NewGuid().ToString("N").Substring(0, 16);
                string sign = GenerateSign(_appKey, _appSecret, timestamp, nonce);

                _webSocket.Options.SetRequestHeader("x-app-key", _appKey);
                _webSocket.Options.SetRequestHeader("x-client-id", _clientId);
                _webSocket.Options.SetRequestHeader("x-timestamp", timestamp);
                _webSocket.Options.SetRequestHeader("x-nonce", nonce);
                _webSocket.Options.SetRequestHeader("x-sign", sign);
                _webSocket.Options.SetRequestHeader("x-sdk-version", "dotnet-0.1.0");

                Console.WriteLine($"[GatewayClient] Connecting to {wsUrl} ...");
                await _webSocket.ConnectAsync(new Uri(wsUrl), cancellationToken);
                Console.WriteLine("[GatewayClient] Connected successfully!");
                _attempt = 0; // reset attempt counter on success

                var buffer = new byte[8192 * 4];
                while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine($"[GatewayClient] WebSocket closed by server: {result.CloseStatusDescription}");
                        break;
                    }
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _ = Task.Run(() => HandleMessageAsync(message, cancellationToken));
                    }
                }
            }
        }

        private async Task HandleMessageAsync(string json, CancellationToken cancellationToken)
        {
            try
            {
                var frame = JsonSerializer.Deserialize<EventFrame>(json);
                if (frame == null || string.IsNullOrEmpty(frame.MsgId)) return;

                string payload = frame.Payload ?? "";
                string msgType = frame.MsgType ?? "";

                // Attempt to check if payload is encrypted
                try
                {
                    var raw = JsonSerializer.Deserialize<RawPayload>(payload);
                    if (raw != null && !string.IsNullOrEmpty(raw.EncryptMsg))
                    {
                        string decrypted = CryptoUtils.DecryptAES(raw.EncryptMsg, _encryptKey);
                        payload = decrypted;
                        // Use inner msgType if provided, else keep outer
                        if (!string.IsNullOrEmpty(raw.MsgType))
                        {
                            msgType = raw.MsgType;
                        }
                    }
                }
                catch
                {
                    // Not a standard encrypted payload, proceed with raw payload
                }

                bool success = false;
                if (_dispatcher != null)
                {
                    success = _dispatcher.Dispatch(msgType, payload);
                }

                await SendAckAsync(frame.MsgId, success, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GatewayClient] Message handling error: {ex.Message}");
            }
        }

        private async Task SendAckAsync(string msgId, bool success, CancellationToken cancellationToken)
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open) return;

            var ack = new AckFrame
            {
                MsgId = msgId,
                Code = success ? 200 : 500,
                Message = success ? "success" : "failure",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            string json = JsonSerializer.Serialize(ack);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            try
            {
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GatewayClient] Failed to send ACK for {msgId}: {ex.Message}");
            }
        }

        private string GenerateSign(string appKey, string appSecret, string timestamp, string nonce)
        {
            string raw = $"{appKey}{timestamp}{nonce}{appSecret}";
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(raw));
                StringBuilder sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public class Builder
        {
            private string? _appKey;
            private string? _appSecret;
            private string? _encryptKey;
            private string? _gatewayUrl;

            public Builder AppKey(string appKey)
            {
                _appKey = appKey;
                return this;
            }

            public Builder AppSecret(string appSecret)
            {
                _appSecret = appSecret;
                return this;
            }

            public Builder EncryptKey(string encryptKey)
            {
                _encryptKey = encryptKey;
                return this;
            }

            public Builder GatewayUrl(string gatewayUrl)
            {
                _gatewayUrl = gatewayUrl;
                return this;
            }

            public GatewayClient Build()
            {
                if (string.IsNullOrEmpty(_appKey)) throw new ArgumentNullException(nameof(_appKey));
                if (string.IsNullOrEmpty(_appSecret)) throw new ArgumentNullException(nameof(_appSecret));

                return new GatewayClient(_appKey, _appSecret, _encryptKey, _gatewayUrl);
            }
        }
    }
}
