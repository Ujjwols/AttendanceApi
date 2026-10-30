using System.Text.Json.Serialization;

public sealed class UpsertSchoolRequest
{
    [JsonPropertyName("schoolCode")]
    public string? SchoolCode { get; set; }

    [JsonPropertyName("schoolName")]
    public string? SchoolName { get; set; }
}
