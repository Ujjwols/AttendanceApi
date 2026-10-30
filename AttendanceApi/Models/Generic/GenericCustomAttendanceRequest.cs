using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class GenericCustomAttendanceRequest
{
    [JsonPropertyName("batchCode")]
    public string BatchCode { get; set; }

    [JsonPropertyName("sentAtUtc")]
    public DateTimeOffset SentAtUtc { get; set; }

    [JsonPropertyName("punches")]
    public List<GenericCustomPunch> Punches { get; set; }
}
