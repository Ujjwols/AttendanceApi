using System;
using System.ComponentModel.DataAnnotations;

public class ExternalEmployee
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string StaffId { get; set; }

    [MaxLength(200)]
    public string? FullName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset? UpdatedOn { get; set; }
}
