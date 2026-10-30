using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class UhfWebhookEvent
{
    [JsonPropertyName("eventId")]
    public string EventId { get; set; }

    [JsonPropertyName("studentNumber")]
    public string StudentNumber { get; set; }

    [JsonPropertyName("punchTime")]
    public DateTimeOffset PunchTime { get; set; }

    [JsonPropertyName("punchStatus")]
    public string PunchStatus { get; set; }

    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; }

    [JsonPropertyName("deviceName")]
    public string DeviceName { get; set; }
}
