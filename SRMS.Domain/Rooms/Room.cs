using SRMS.Domain.Abstractions;
using SRMS.Domain.Residences;
using SRMS.Domain.Rooms.Enums;
using SRMS.Domain.Students;
using SRMS.Domain.Reservations;
using SRMS.Domain.ValueObjects;

namespace SRMS.Domain.Rooms;

public class Room : Entity
{
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public RoomType RoomType { get; set; }
    
    // Amenities
    public bool HasPrivateBathroom { get; set; }
    public bool HasAirConditioning { get; set; }
    public bool HasHeating { get; set; }
    public bool HasWifi { get; set; }
    public bool HasDesk { get; set; }
    public bool HasWardrobe { get; set; }
    public bool HasBalcony { get; set; }
    
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public bool IsFull => OccupiedBeds >= TotalBeds;

    // Pricing
    public Money? MonthlyRent { get; set; }

    // Navigation Properties
    public Guid ResidenceId { get; set; }
    public Residence Residence { get; set; } = null!;
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}