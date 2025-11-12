using SRMS.Domain.Payments.Enums;

namespace SRMS.Application.Payments.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string Period { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime CreatedAt { get; set; }
}