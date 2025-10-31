namespace SRMS.Domain.Payments.Enums;

/// <summary>
/// Student Entity - الطالب
/// </summary>
public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}