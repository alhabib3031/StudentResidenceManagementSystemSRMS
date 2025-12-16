using SRMS.Domain.Students;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Complaints;

namespace SRMS.Application.Complaints.DTOs;

/// <summary>
/// This class is used to update complaint status Inside the system
/// </summary>
public class ComplaintUpdateDto
{
    public DateTime Timestamp { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ComplaintStatus? NewStatus { get; set; }
}