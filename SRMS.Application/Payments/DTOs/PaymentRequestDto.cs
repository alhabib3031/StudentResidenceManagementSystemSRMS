namespace SRMS.Application.Payments.DTOs;

public class PaymentRequestDto
{
    public Guid StudentId { get; set; }
    public Guid RoomId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR"; // Default currency
    public string Description { get; set; } = "Room Reservation Payment";
}