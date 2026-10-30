using System.ComponentModel.DataAnnotations;

/// <summary>School reference data maintained by the mock API, not supplied by attendance senders.</summary>
public sealed class School
{
    [Key]
    [MaxLength(50)]
    public string SchoolCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string SchoolName { get; set; } = string.Empty;
}
