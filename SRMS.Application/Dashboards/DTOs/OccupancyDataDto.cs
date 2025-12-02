namespace SRMS.Application.Dashboards.DTOs;

public class OccupancyDataDto
{
    public string Name { get; set; } = "";
    public int Total { get; set; }
    public int Occupied { get; set; }
    public double Rate { get; set; }
}