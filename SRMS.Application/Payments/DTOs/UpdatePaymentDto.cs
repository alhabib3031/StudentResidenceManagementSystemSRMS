using System.ComponentModel.DataAnnotations;
using SRMS.Domain.Payments.Enums;

namespace SRMS.Application.Payments.DTOs;

public class UpdatePaymentDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    
    public string Currency { get; set; } = "LYD";

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public PaymentStatus Status { get; set; }

    public DateTime? PaidAt { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentMethod { get; set; }

    public decimal? LateFeeAmount { get; set; }
    public string? LateFeeСurrency { get; set; } = "LYD";

    public string? Notes { get; set; }
}