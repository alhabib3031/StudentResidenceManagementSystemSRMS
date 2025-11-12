namespace SRMS.Domain.Payments.Enums;

/// <summary>
/// Student Entity - الطالب
/// </summary>
public enum PaymentStatus
{
    Pending = 0,        // في انتظار الدفع
    Paid = 1,           // تم الدفع
    Overdue = 2,        // متأخر
    Cancelled = 3,      // ملغى
    Refunded = 4,       // تم الاسترداد
    PartiallyPaid = 5   // دفع جزئي
}