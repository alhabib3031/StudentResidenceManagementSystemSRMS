using System.ComponentModel.DataAnnotations;
using SRMS.Domain.Complaints.Enums;

namespace SRMS.Application.Complaints.DTOs;

public class UpdateComplaintDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public ComplaintCategory Category { get; set; }

    [Required]
    public ComplaintPriority Priority { get; set; }

    [Required]
    public ComplaintStatus Status { get; set; }

    public Guid? AssignedTo { get; set; }
    public string? Resolution { get; set; }
    public string? UpdatesJson { get; set; }
}