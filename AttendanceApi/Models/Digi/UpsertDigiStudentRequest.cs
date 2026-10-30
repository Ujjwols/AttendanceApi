using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class UpsertDigiStudentRequest
{
    [JsonPropertyName("studentId")]
    public string StudentId { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }
}
