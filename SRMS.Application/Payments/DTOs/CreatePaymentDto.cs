using System.ComponentModel.DataAnnotations;

namespace SRMS.Application.Payments.DTOs;

// ============================================================
// DTOs
// ============================================================
public class CreatePaymentDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    
    public string Currency { get; set; } = "LYD";

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [Range(2020, 2100)]
    public int Year { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public decimal? LateFeeAmount { get; set; }
    public string? LateFeeCurrency { get; set; } = "LYD";

    public string? Notes { get; set; }
}