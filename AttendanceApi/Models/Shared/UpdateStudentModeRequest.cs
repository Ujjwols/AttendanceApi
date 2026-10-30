using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class UpdateStudentModeRequest
{
    [JsonPropertyName("responseMode")]
    public string ResponseMode { get; set; }
}
