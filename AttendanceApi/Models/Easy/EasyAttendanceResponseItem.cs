using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class EasyAttendanceResponseItem
{
    [JsonPropertyName("StatusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("Message")]
    public string Message { get; set; }

    [JsonPropertyName("NotificationToken")]
    public string NotificationToken { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; }

    [JsonPropertyName("Description")]
    public string Description { get; set; }

    [JsonPropertyName("Source")]
    public string Source { get; set; }
}
