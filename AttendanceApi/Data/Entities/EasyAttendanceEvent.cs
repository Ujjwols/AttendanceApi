using System;
using System.ComponentModel.DataAnnotations;

public class EasyAttendanceEvent
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string StaffId { get; set; }

    [Required]
    [MaxLength(20)]
    public string AttenDate { get; set; }

    [Required]
    [MaxLength(30)]
    public string AttenTime { get; set; }

    [MaxLength(20)]
    public string NepDate { get; set; }

    [Required]
    [MaxLength(100)]
    public string ComId { get; set; }

    public string Remarks { get; set; }

    [MaxLength(100)]
    public string MachineId { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }
}
