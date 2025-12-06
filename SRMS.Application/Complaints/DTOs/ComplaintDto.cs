using SRMS.Domain.Complaints.Enums;

namespace SRMS.Application.Complaints.DTOs;

public class ComplaintDto
{
    public Guid Id { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; // Added
    public ComplaintCategory Category { get; set; }
    public ComplaintPriority Priority { get; set; }
    public ComplaintStatus Status { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }

    // Resolution Details
    public string? ResolutionNotes { get; set; } // Mapped from Resolution
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByManagerId { get; set; } // Mapped from ResolvedBy
}