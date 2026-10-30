using System;
using System.ComponentModel.DataAnnotations;

public class DigiStaffAttendanceEvent
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string DeviceNumber { get; set; }

    [Required]
    [MaxLength(100)]
    public string EmployeeId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }
}
