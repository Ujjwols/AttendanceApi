using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class EasyAttendanceResponse
{
    [JsonPropertyName("AttenLst")]
    public List<EasyAttendanceResponseItem> AttenLst { get; set; }

    [JsonPropertyName("StatusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("Message")]
    public string Message { get; set; }
}
