using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class DigiStaffBulkAttendanceRequest
{
    [JsonPropertyName("deviceNumber")]
    public string DeviceNumber { get; set; }

    [JsonPropertyName("attendanceRecords")]
    public List<DigiStaffBulkAttendanceRecordRequest> AttendanceRecords { get; set; }
}
