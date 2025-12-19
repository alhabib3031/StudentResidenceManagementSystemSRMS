using SRMS.Domain.Payments.Enums;

namespace SRMS.Application.Payments.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "LYD";
    public string Description { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string Period { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentMethod { get; set; }
    public string Reference { get; set; } = string.Empty;
    public bool IsOverdue => Status != PaymentStatus.Paid && DueDate < DateTime.UtcNow; // محسوبة تلقائيات بدلا من تخزينها في قاعدة البيانات
    public DateTime CreatedAt { get; set; }
}