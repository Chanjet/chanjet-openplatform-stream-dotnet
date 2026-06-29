using System;
using System.Collections.Generic;
using System.Text.Json;
using Chanjet.Connector.Sdk.Protocol;

namespace Chanjet.Connector.Sdk
{
    public class MessageDispatcher
    {
        private readonly Dictionary<string, Func<string, bool>> _handlers = new();

        public void Register<T>(string msgType, Func<T, bool> handler) where T : BaseMessage
        {
            _handlers[msgType] = (payload) =>
            {
                var msg = JsonSerializer.Deserialize<T>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (msg != null)
                {
                    return handler(msg);
                }
                return false;
            };
        }

        public void OnAppTicket(Func<AppTicketMessage, bool> handler)
        {
            Register("APP_TICKET", handler);
        }

        public void OnEntAuthCode(Func<EntAuthCodeMessage, bool> handler)
        {
            Register("TEMP_AUTH_CODE", handler);
        }

        public void OnEntUnauth(Func<EntUnauthMessage, bool> handler)
        {
            Register("APP_CANCEL_AUTH", handler);
            // Notice: other types like APP_CANCEL_OPEN might also share the same structure, 
            // but we provide a separate semantic method if needed, or register them manually.
        }

        public void OnAppCancelOpen(Func<AppCancelOpenMessage, bool> handler)
        {
            Register("APP_CANCEL_OPEN", handler);
        }

        public void OnOrderStatus(Func<OrderStatusMessage, bool> handler)
        {
            Register("PAY_ORDER_SUCCESS", handler);
        }

        public void OnAppNotice(string boName, string transactionTypeEnum, Func<AppNoticeMessage, JsonElement, bool> handler)
        {
            string key = $"APP_NOTICE_{boName}_{transactionTypeEnum}";
            _handlers[key] = (payload) =>
            {
                var msg = JsonSerializer.Deserialize<AppNoticeMessage>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (msg != null)
                {
                    using var document = JsonDocument.Parse(payload);
                    var bizContent = document.RootElement.GetProperty("bizContent");
                    return handler(msg, bizContent);
                }
                return false;
            };
        }

        public void OnAppNotice(string boName, Func<AppNoticeMessage, JsonElement, bool> handler)
        {
            string key = $"APP_NOTICE_{boName}";
            _handlers[key] = (payload) =>
            {
                var msg = JsonSerializer.Deserialize<AppNoticeMessage>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (msg != null)
                {
                    using var document = JsonDocument.Parse(payload);
                    var bizContent = document.RootElement.GetProperty("bizContent");
                    return handler(msg, bizContent);
                }
                return false;
            };
        }

        public bool Dispatch(string msgType, string payload)
        {
            // Specifically handle APP_NOTICE composite keys
            if (msgType == "APP_NOTICE")
            {
                using var doc = JsonDocument.Parse(payload);
                if (doc.RootElement.TryGetProperty("bizContent", out var bizContent))
                {
                    string boName = "";
                    if (bizContent.TryGetProperty("boName", out var boNameProp))
                    {
                        boName = boNameProp.GetString() ?? "";
                    }

                    string transType = "";
                    if (bizContent.TryGetProperty("transactionTypeEnum", out var transTypeProp))
                    {
                        transType = transTypeProp.GetString() ?? "";
                    }

                    string exactKey = $"APP_NOTICE_{boName}_{transType}";
                    if (_handlers.TryGetValue(exactKey, out var exactHandler))
                    {
                        return exactHandler(payload);
                    }

                    string partialKey = $"APP_NOTICE_{boName}";
                    if (_handlers.TryGetValue(partialKey, out var partialHandler))
                    {
                        return partialHandler(payload);
                    }
                }
            }

            if (_handlers.TryGetValue(msgType, out var handler))
            {
                return handler(payload);
            }

            Console.WriteLine($"[Warning] No handler registered for msgType: {msgType}. Silently ignoring.");
            return true;
        }
    }
}
