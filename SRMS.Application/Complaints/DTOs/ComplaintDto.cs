using SRMS.Domain.Students;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Complaints;

namespace SRMS.Application.Complaints.DTOs;

public class ComplaintDto
{
    public Guid Id { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; // Added
    public string ComplaintType { get; set; } = string.Empty;
    public ComplaintPriority Priority { get; set; }
    public ComplaintStatus Status { get; set; }
    public Guid ReservationId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }

    // Resolution Details
    public string? ResolutionNotes { get; set; } // Mapped from Resolution
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByManagerId { get; set; } // Mapped from ResolvedBy
}