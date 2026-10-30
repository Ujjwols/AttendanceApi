using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class UhfWebhookResult
{
    [JsonPropertyName("eventId")]
    public string EventId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("reference")]
    public string Reference { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}
