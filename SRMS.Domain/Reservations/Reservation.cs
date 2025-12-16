using SRMS.Domain.Abstractions;
using SRMS.Domain.Rooms;
using SRMS.Domain.Students;
using SRMS.Domain.Reservations.Enums;
using SRMS.Domain.Complaints;
using SRMS.Domain.Payments;

namespace SRMS.Domain.Reservations;

/// <summary>
/// Intermediate entity for the Many-to-Many relationship between Student and Room, representing a Reservation.
/// </summary>
public class Reservation : Entity
{
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    // Navigation Properties
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
