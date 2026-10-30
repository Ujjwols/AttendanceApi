using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class UhfWebhookResponse
{
    [JsonPropertyName("acknowledged")]
    public bool Acknowledged { get; set; }

    [JsonPropertyName("acknowledgementReference")]
    public string AcknowledgementReference { get; set; }

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; }

    [JsonPropertyName("receivedAt")]
    public DateTimeOffset ReceivedAt { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("results")]
    public List<UhfWebhookResult> Results { get; set; }
}
