using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class EasyAttendanceRequestItem
{
    [JsonPropertyName("StaffID")]
    public string StaffID { get; set; }

    [JsonPropertyName("AttenDate")]
    public string AttenDate { get; set; }

    [JsonPropertyName("AttenTime")]
    public string AttenTime { get; set; }

    [JsonPropertyName("NepDate")]
    public string NepDate { get; set; }
}
