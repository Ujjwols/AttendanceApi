using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class UpsertStudentRequest
{
    [JsonPropertyName("studentNumber")]
    public string StudentNumber { get; set; }

    [JsonPropertyName("fullName")]
    public string FullName { get; set; }

    [JsonPropertyName("className")]
    public string ClassName { get; set; }

    [JsonPropertyName("rollNumber")]
    public string RollNumber { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("responseMode")]
    public string ResponseMode { get; set; }
}
