using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class DigiStaffAttendanceRequest
{
    [JsonPropertyName("deviceNumber")]
    public string DeviceNumber { get; set; }

    [JsonPropertyName("employeeId")]
    public string EmployeeId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}
