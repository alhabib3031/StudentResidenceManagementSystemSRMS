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
    public int AvailableBeds => TotalBeds - OccupiedBeds; // Calculated property
    public bool IsFull { get; set; }
    public RoomStatus Status { get; set; }
    public bool IsActive { get; set; }
    public decimal? MonthlyRentAmount { get; set; }
    public string? MonthlyRentCurrency { get; set; }
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

    // Auditing Properties
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}