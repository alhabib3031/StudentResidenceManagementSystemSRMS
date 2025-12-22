namespace SRMS.Application.Reservations.DTOs;

public class ReservationRequestDto
{
    public Guid StudentId { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal AgreedPrice { get; set; } // The price student agreed to pay
    public Guid PaymentId { get; set; } // The ID of the successful payment
}