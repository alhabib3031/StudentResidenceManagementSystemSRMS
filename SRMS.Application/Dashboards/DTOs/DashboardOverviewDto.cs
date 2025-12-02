namespace SRMS.Application.Dashboards.DTOs;

public class DashboardOverviewDto
{
    public int TotalResidences { get; set; }
    public int ActiveManagers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalCapacity { get; set; }
    public int OccupiedCapacity { get; set; }
    public double OccupancyRate { get; set; }
    public int PendingComplaints { get; set; }
    public int ResolvedComplaints { get; set; }
    public int ResidencesGrowth { get; set; }
    public double ManagerAvailability { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public double RevenueGrowth { get; set; }
}