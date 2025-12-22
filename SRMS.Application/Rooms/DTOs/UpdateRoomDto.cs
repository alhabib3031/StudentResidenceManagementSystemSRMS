using SRMS.Domain.Rooms.Enums;

namespace SRMS.Application.Rooms.DTOs;

public class UpdateRoomDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public RoomType RoomType { get; set; }
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public decimal MonthlyRentAmount { get; set; } // Using decimal for currency
    public string MonthlyRentCurrency { get; set; } = "SAR";
    public Guid ResidenceId { get; set; }

    // Amenities
    public bool HasPrivateBathroom { get; set; }
    public bool HasAirConditioning { get; set; }
    public bool HasHeating { get; set; }
    public bool HasWifi { get; set; }
    public bool HasDesk { get; set; }
    public bool HasWardrobe { get; set; }
    public bool HasBalcony { get; set; }
    public bool IsAvailable { get; set; }
}