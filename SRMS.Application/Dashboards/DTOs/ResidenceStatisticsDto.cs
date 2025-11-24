namespace SRMS.Application.Dashboards.DTOs;

public class ResidenceStatisticsDto
{
    public int TotalResidences { get; set; }
    public int TotalCapacity { get; set; }
    public int AvailableCapacity { get; set; }
    public int OccupiedCapacity { get; set; }
    public double AverageOccupancy { get; set; }
    public int FullResidences { get; set; }
}