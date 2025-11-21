using SRMS.Domain.Payments.Enums;

namespace SRMS.Application.Payments.Extensions;

public static class EnumExtensions
{
    // ========== Payment Status ==========
    public static string ToArabic(this PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "في انتظار الدفع",
        PaymentStatus.Paid => "مدفوع",
        PaymentStatus.Overdue => "متأخر",
        PaymentStatus.Cancelled => "ملغى",
        PaymentStatus.Refunded => "تم الاسترداد",
        PaymentStatus.PartiallyPaid => "دفع جزئي",
        _ => status.ToString()
    };
    
    public static string GetBadgeClass(this PaymentStatus status) => status switch
    {
        PaymentStatus.Paid => "badge-success",
        PaymentStatus.Pending => "badge-warning",
        PaymentStatus.Overdue => "badge-danger",
        PaymentStatus.Cancelled => "badge-secondary",
        PaymentStatus.Refunded => "badge-info",
        PaymentStatus.PartiallyPaid => "badge-warning",
        _ => "badge-secondary"
    };
}