using SRMS.Domain.Complaints.Enums;

namespace SRMS.Application.Complaints.DTOs;

public class ComplaintDto
{
    public Guid Id { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ComplaintCategory Category { get; set; }
    public ComplaintPriority Priority { get; set; }
    public ComplaintStatus Status { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
}