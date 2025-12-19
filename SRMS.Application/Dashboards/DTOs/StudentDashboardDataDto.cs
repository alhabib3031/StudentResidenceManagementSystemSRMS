using SRMS.Domain.Students.Enums;

namespace SRMS.Application.Dashboards.DTOs;

public class StudentDashboardDataDto
{
    public string? RoomNumber { get; set; }
    public string? ResidenceName { get; set; }
    public decimal DueAmount { get; set; }
    public int ActiveComplaintsCount { get; set; }
    public StudentStatus Status { get; set; }
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty; // e.g. "Notification", "Payment", "Complaint"
}
