using SRMS.Domain.Payments.Enums;

namespace SRMS.Application.Payments.DTOs;

public class PaymentDetailsDto
{
    public Guid Id { get; set; }
    
    // Student
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string? StudentEmail { get; set; }
    public string? StudentPhone { get; set; }
    
    // Payment Info
    public decimal AmountValue { get; set; }
    public string AmountCurrency { get; set; } = "LYD";
    public string AmountFormatted { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    
    // Period
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    
    // Payment Details
    public DateTime? PaidAt { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentMethod { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    
    // Late Fee
    public decimal? LateFeeValue { get; set; }
    public string? LateFeeCurrency { get; set; }
    public string? LateFeeFormatted { get; set; }
    
    // Status Info
    public bool IsOverdue { get; set; }
    public int DaysOverdue { get; set; }
    
    public string? Notes { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
