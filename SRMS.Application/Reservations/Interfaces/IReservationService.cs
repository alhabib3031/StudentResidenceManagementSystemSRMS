using SRMS.Application.Reservations.DTOs;
using SRMS.Domain.Reservations;
using SRMS.Domain.Reservations.Enums;
using SRMS.Domain.ValueObjects; // For Money

namespace SRMS.Application.Reservations.Interfaces;

public interface IReservationService
{
    Task<Reservation> CreateReservationAsync(ReservationRequestDto request);
    Task<Reservation?> GetReservationByIdAsync(Guid reservationId);
    Task<List<Reservation>> GetStudentReservationsAsync(Guid studentId);
    Task<Reservation> UpdateReservationStatusAsync(Guid reservationId, ReservationStatus newStatus);

    // New methods for student room booking flow
    Task<IEnumerable<ResidenceDto>> GetAvailableResidencesAsync();
    Task<IEnumerable<RoomAvailabilityDto>> GetVacantRoomsByResidenceAsync(Guid residenceId);
    Task<ReserveRoomResponse> ReserveRoomAsync(ReserveRoomRequest request);
}
