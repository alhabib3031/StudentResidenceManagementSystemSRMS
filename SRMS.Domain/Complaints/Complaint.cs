using SRMS.Domain.Abstractions;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Students;

namespace SRMS.Domain.Complaints;

/// <summary>
/// Complaint Entity - الشكوى (حامل للخصائص فقط)
/// </summary>
public class Complaint : Entity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ComplaintNumber { get; set; } = string.Empty;
    
    // Student
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    
    // Category & Priority
    public ComplaintCategory Category { get; set; }
    public ComplaintPriority Priority { get; set; }
    public ComplaintStatus Status { get; set; }
    
    // Assignment
    public Guid? AssignedTo { get; set; }
    public DateTime? AssignedAt { get; set; }
    
    // Resolution
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
    
    // Attachments (stored as JSON or separate table)
    public string? AttachmentsJson { get; set; }
    
    // Updates (stored as JSON or separate table)
    public string? UpdatesJson { get; set; }
}
