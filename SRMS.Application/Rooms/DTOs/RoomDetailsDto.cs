using SRMS.Domain.Rooms.Enums;

namespace SRMS.Application.Rooms.DTOs;

public class RoomDetailsDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public RoomType RoomType { get; set; }
    
    // Beds
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int AvailableBeds { get; set; }
    public bool IsFull { get; set; }
    
    // Residence
    public Guid ResidenceId { get; set; }
    public string ResidenceName { get; set; } = string.Empty;
    public string? ResidenceAddress { get; set; }
    
    // Amenities
    public bool HasPrivateBathroom { get; set; }
    public bool HasAirConditioning { get; set; }
    public bool HasHeating { get; set; }
    public bool HasWifi { get; set; }
    public bool HasDesk { get; set; }
    public bool HasWardrobe { get; set; }
    public bool HasBalcony { get; set; }
    public List<string> AmenitiesList { get; set; } = new();
    
    // Status
    public bool IsActive { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Students
    public int StudentsCount { get; set; }
    public List<RoomStudentDto> Students { get; set; } = new();
}