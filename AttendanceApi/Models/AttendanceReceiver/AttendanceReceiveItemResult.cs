public sealed class AttendanceReceiveItemResult
{
    public int Index { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? SchoolCode { get; set; }
    public string? SchoolName { get; set; }
    public int? AttendanceEventId { get; set; }
}
