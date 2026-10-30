using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class DigiStudentBulkAttendanceRequest
{
    [JsonPropertyName("deviceNumber")]
    public string DeviceNumber { get; set; }

    [JsonPropertyName("attendanceRecords")]
    public List<DigiStudentBulkAttendanceRecordRequest> AttendanceRecords { get; set; }
}
