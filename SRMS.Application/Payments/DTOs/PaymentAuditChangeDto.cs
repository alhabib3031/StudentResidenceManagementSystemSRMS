using SRMS.Domain.Payments.Enums;

namespace SRMS.Application.Payments.DTOs;

public class PaymentAuditChangeDto
{
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
}