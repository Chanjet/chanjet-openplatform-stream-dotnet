using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Chanjet.Connector.Sdk.Protocol
{
    public class EventFrame
    {
        [JsonPropertyName("msg_type")]
        public string? MsgType { get; set; }

        [JsonPropertyName("msg_id")]
        public string? MsgId { get; set; }

        [JsonPropertyName("trace_id")]
        public string? TraceId { get; set; }

        [JsonPropertyName("app_key")]
        public string? AppKey { get; set; }

        [JsonPropertyName("target_client_id")]
        public string? TargetClientId { get; set; }

        [JsonPropertyName("headers")]
        public Dictionary<string, string>? Headers { get; set; }

        [JsonPropertyName("payload")]
        public string? Payload { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }

    public class AckFrame
    {
        [JsonPropertyName("msg_id")]
        public string? MsgId { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }

    public class BaseMessage
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("msgId")]
        public string? MsgId { get; set; }

        [JsonPropertyName("msgType")]
        public string? MsgType { get; set; }

        [JsonPropertyName("appKey")]
        public string? AppKey { get; set; }

        [JsonPropertyName("appId")]
        public string? AppId { get; set; }

        [JsonPropertyName("time")]
        public string? Timestamp { get; set; }

        [JsonPropertyName("headers")]
        public Dictionary<string, string>? Headers { get; set; }
    }

    public class AppTicketMessage : BaseMessage
    {
        [JsonPropertyName("bizContent")]
        public AppTicketContent? BizContent { get; set; }

        public class AppTicketContent
        {
            [JsonPropertyName("appTicket")]
            public string? AppTicket { get; set; }
        }
    }

    public class EntAuthCodeMessage : BaseMessage
    {
        [JsonPropertyName("bizContent")]
        public EntAuthCodeContent? BizContent { get; set; }

        public class EntAuthCodeContent
        {
            [JsonPropertyName("tempAuthCode")]
            public string? TempAuthCode { get; set; }

            [JsonPropertyName("state")]
            public string? State { get; set; }
        }
    }

    public class EntUnauthMessage : BaseMessage
    {
        [JsonPropertyName("bizContent")]
        public EntUnauthContent? BizContent { get; set; }

        public class EntUnauthContent
        {
            [JsonPropertyName("orgId")]
            public string? OrgId { get; set; }

            [JsonPropertyName("userId")]
            public string? UserId { get; set; }
        }
    }

    public class AppCancelOpenMessage : BaseMessage
    {
        [JsonPropertyName("bizContent")]
        public AppCancelOpenContent? BizContent { get; set; }

        public class AppCancelOpenContent
        {
            [JsonPropertyName("appId")]
            public string? AppId { get; set; }
        }
    }

    public class OrderStatusMessage : BaseMessage
    {
        [JsonPropertyName("bizContent")]
        public OrderStatusContent? BizContent { get; set; }

        public class OrderStatusContent
        {
            [JsonPropertyName("orderNo")]
            public string? OrderNo { get; set; }

            [JsonPropertyName("orgId")]
            public string? OrgId { get; set; }

            [JsonPropertyName("detail")]
            public OrderStatusDetail? Detail { get; set; }
        }

        public class OrderStatusDetail
        {
            [JsonPropertyName("payTotal")]
            public double PayTotal { get; set; }
            // Additional fields can be added here
        }
    }

    public class AppNoticeMessage : BaseMessage
    {
        [JsonPropertyName("bizContent")]
        public AppNoticeContent? BizContent { get; set; }

        public class AppNoticeContent
        {
            [JsonPropertyName("boName")]
            public string? BoName { get; set; }

            [JsonPropertyName("transactionTypeEnum")]
            public string? TransactionTypeEnum { get; set; }
        }
    }

    public class RawPayload
    {
        [JsonPropertyName("encryptMsg")]
        public string? EncryptMsg { get; set; }

        [JsonPropertyName("msgType")]
        public string? MsgType { get; set; }
    }
}
