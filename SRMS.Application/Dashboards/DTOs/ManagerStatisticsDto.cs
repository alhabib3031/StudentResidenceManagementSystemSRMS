namespace SRMS.Application.Dashboards.DTOs;

public class ManagerStatisticsDto
{
    public int TotalManagers { get; set; }
    public int ActiveManagers { get; set; }
    public int OnLeaveManagers { get; set; }
    public int SuspendedManagers { get; set; }
    public double AverageWorkload { get; set; }
}