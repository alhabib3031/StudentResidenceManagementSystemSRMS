using SRMS.Domain.Rooms.Enums;
using SRMS.Domain.ValueObjects;

namespace SRMS.Application.Reservations.DTOs;

public record ResidenceDto(Guid Id, string Name, string Location);
public record RoomAvailabilityDto(Guid Id, string RoomNumber, int Floor, RoomType RoomType, int TotalBeds, int OccupiedBeds, Money? MonthlyRent, RoomStatus Status);

public record ReserveRoomRequest(
    Guid StudentId,
    Guid RoomId,
    Guid ResidenceId,
    DateTime StartDate,
    DateTime EndDate
);

public record ReserveRoomResponse(
    Guid ReservationId,
    Guid PaymentId,
    Money AmountPaid,
    string Status
);
