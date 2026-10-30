using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class UpsertExternalEmployeeRequest
{
    [JsonPropertyName("staffId")]
    public string StaffId { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }
}
