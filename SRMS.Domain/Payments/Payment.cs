using SRMS.Domain.Abstractions;
using SRMS.Domain.Payments.Enums;
using SRMS.Domain.Students;
using SRMS.Domain.Reservations;
using SRMS.Domain.ValueObjects;

namespace SRMS.Domain.Payments;

/// <summary>
/// Payment Entity - الدفعة (حامل للخصائص فقط)
/// </summary>
public class Payment : Entity
{
    public Guid? ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;
    
    public Money Amount { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    
    public DateTime? PaidAt { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentMethod { get; set; }

    public string PaymentReference { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime DueDate { get; set; }
    
    public Money? LateFee { get; set; }
    public string? Notes { get; set; }
}