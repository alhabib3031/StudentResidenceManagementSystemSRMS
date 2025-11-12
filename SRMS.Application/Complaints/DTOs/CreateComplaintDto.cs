using System.ComponentModel.DataAnnotations;
using SRMS.Domain.Complaints.Enums;

namespace SRMS.Application.Complaints.DTOs;

public class CreateComplaintDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public ComplaintCategory Category { get; set; }

    public ComplaintPriority Priority { get; set; } = ComplaintPriority.Medium;

    public string? AttachmentsJson { get; set; }
}