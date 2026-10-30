using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class EasyTokenResponse
{
    [JsonPropertyName("StatusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("Message")]
    public string Message { get; set; }

    [JsonPropertyName("Token")]
    public string Token { get; set; }

    [JsonPropertyName("ExpiryMinutes")]
    public double ExpiryMinutes { get; set; }
}
