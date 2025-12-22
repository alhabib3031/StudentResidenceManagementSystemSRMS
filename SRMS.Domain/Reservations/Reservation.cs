using SRMS.Domain.Abstractions;
using SRMS.Domain.Rooms;
using SRMS.Domain.Students;
using SRMS.Domain.ValueObjects;
using SRMS.Domain.Reservations.Enums;
using System.Security.AccessControl;
using SRMS.Domain.Complaints; // Added for Complaints navigation property
using SRMS.Domain.Payments;   // Added for Payments navigation property

namespace SRMS.Domain.Reservations;

public class Reservation : Entity
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public Guid ResidenceId { get; set; } // New property

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime ReservationDate { get; set; }
    public bool IsPaid { get; set; }
    public Money? TotalAmount { get; set; }
    public ReservationStatus Status { get; set; }
    
    // Navigation properties for relationships
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    
}