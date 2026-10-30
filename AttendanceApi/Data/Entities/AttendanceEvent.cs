using System;
using System.ComponentModel.DataAnnotations;

/// <summary>A received student punch; SchoolName is copied from SchoolsList at receipt time.</summary>
public sealed class AttendanceEvent
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string SchoolCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SchoolName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string StudentName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ClassName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Classroom { get; set; } = string.Empty;

    public DateTimeOffset PunchTime { get; set; }

    [Required]
    [MaxLength(50)]
    public string PunchStatus { get; set; } = string.Empty;

    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}
