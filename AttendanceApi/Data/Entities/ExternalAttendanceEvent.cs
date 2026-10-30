using System;
using System.ComponentModel.DataAnnotations;

public class ExternalAttendanceEvent
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string RequestId { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventId { get; set; }

    [Required]
    [MaxLength(100)]
    public string StudentNumber { get; set; }

    public DateTimeOffset PunchTime { get; set; }

    [Required]
    [MaxLength(10)]
    public string PunchStatus { get; set; }

    [MaxLength(100)]
    public string DeviceType { get; set; }

    [MaxLength(200)]
    public string DeviceName { get; set; }

    [Required]
    [MaxLength(50)]
    public string ResultStatus { get; set; }

    public string ResultMessage { get; set; }

    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedOn { get; set; }
}
