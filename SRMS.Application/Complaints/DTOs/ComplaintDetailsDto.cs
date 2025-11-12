using SRMS.Domain.Complaints.Enums;

namespace SRMS.Application.Complaints.DTOs;

public class ComplaintDetailsDto
{
    public Guid Id { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Student
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentEmail { get; set; }
    public string? StudentPhone { get; set; }
    public string? StudentRoomNumber { get; set; }
    
    // Classification
    public ComplaintCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public ComplaintPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    
    // Assignment
    public Guid? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? AssignedAt { get; set; }
    
    // Resolution
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
    public string? ResolvedByName { get; set; }
    public bool IsResolved { get; set; }
    public int? ResolutionDays { get; set; }
    
    // Attachments
    public List<string> Attachments { get; set; } = new();
    
    // Updates History
    public List<ComplaintUpdateDto> Updates { get; set; } = new();
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int DaysOpen { get; set; }
}