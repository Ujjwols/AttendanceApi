using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class EasyAttendanceRequest
{
    [JsonPropertyName("Param")]
    public List<EasyAttendanceRequestItem> Param { get; set; }

    [JsonPropertyName("ComID")]
    public string ComID { get; set; }

    [JsonPropertyName("Remarks")]
    public string Remarks { get; set; }

    [JsonPropertyName("MachineID")]
    public string MachineID { get; set; }
}
