using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class UpdateEasyMockModeRequest
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; }
}
