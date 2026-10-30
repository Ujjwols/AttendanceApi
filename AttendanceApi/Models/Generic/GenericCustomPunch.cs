using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class GenericCustomPunch
{
    [JsonPropertyName("eventKey")]
    public string EventKey { get; set; }

    [JsonPropertyName("personType")]
    public int PersonType { get; set; }

    [JsonPropertyName("personId")]
    public int PersonId { get; set; }

    [JsonPropertyName("movement")]
    public string Movement { get; set; }

    [JsonPropertyName("occurredAtUtc")]
    public DateTimeOffset OccurredAtUtc { get; set; }

    [JsonPropertyName("terminalType")]
    public string TerminalType { get; set; }

    [JsonPropertyName("terminalName")]
    public string TerminalName { get; set; }
}
