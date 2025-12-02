using SRMS.Domain.Rooms.Enums;

namespace SRMS.Application.Rooms.DTOs;

public class RoomDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public RoomType RoomType { get; set; }
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int AvailableBeds { get; set; }
    public bool IsFull { get; set; }
    public string ResidenceName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}