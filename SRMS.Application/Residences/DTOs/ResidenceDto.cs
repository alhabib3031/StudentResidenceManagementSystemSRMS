namespace SRMS.Application.Residences.DTOs;

public class ResidenceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public string? Description { get; set; }
    public int TotalCapacity { get; set; }
    public int AvailableCapacity { get; set; }
    public int CurrentRoomsCount { get; set; }
    public int MaxRoomsCount { get; set; }
    public bool IsFull { get; set; }
}