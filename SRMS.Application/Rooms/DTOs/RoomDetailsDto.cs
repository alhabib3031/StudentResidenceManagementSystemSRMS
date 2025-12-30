using SRMS.Domain.Rooms.Enums;

namespace SRMS.Application.Rooms.DTOs;

public class RoomDetailsDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public RoomType RoomType { get; set; }
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public bool IsFull { get; set; }
    public decimal BaseMonthlyRent { get; set; } // Base rent from Room entity
    public decimal AdjustedMonthlyRent { get; set; } // Price adjusted for student
    public Guid ResidenceId { get; set; }
    public string ResidenceName { get; set; } = string.Empty;

    // Amenities
    public bool HasPrivateBathroom { get; set; }
    public bool HasAirConditioning { get; set; }
    public bool HasHeating { get; set; }
    public bool HasWifi { get; set; }
    public bool HasDesk { get; set; }
    public bool HasWardrobe { get; set; }
    public bool HasBalcony { get; set; }

    // Student specific information that influenced the adjusted price (for display)
    public string StudentStudyLevel { get; set; } = string.Empty;
    public string StudentNationality { get; set; } = string.Empty;
    public List<string> Roommates { get; set; } = new();
}