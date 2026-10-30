using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class UhfWebhookRequest
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; }

    [JsonPropertyName("sentAt")]
    public DateTimeOffset SentAt { get; set; }

    [JsonPropertyName("events")]
    public List<UhfWebhookEvent> Events { get; set; }
}
