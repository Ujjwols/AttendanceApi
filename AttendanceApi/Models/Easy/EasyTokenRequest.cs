using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class EasyTokenRequest
{
    [JsonPropertyName("ComID")]
    public string ComID { get; set; }

    [JsonPropertyName("Login")]
    public string Login { get; set; }

    [JsonPropertyName("Password")]
    public string Password { get; set; }
}
