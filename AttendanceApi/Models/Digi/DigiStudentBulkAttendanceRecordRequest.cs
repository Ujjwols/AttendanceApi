using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class DigiStudentBulkAttendanceRecordRequest
{
    [JsonPropertyName("studentId")]
    public string StudentId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}
