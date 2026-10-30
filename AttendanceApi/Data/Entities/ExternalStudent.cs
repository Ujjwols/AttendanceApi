using System;
using System.ComponentModel.DataAnnotations;

public class ExternalStudent
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string StudentNumber { get; set; }

    [MaxLength(200)]
    public string FullName { get; set; }

    [MaxLength(100)]
    public string ClassName { get; set; }

    [MaxLength(50)]
    public string RollNumber { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    [MaxLength(20)]
    public string ResponseMode { get; set; } = "ACCEPTED";

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedOn { get; set; }
}
