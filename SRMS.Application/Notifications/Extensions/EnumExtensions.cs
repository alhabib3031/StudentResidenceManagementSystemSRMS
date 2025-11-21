using SRMS.Domain.Notifications.Enums;

namespace SRMS.Application.Notifications.Extensions;

public static class EnumExtensions
{
    // ========== Notification Type ==========
    public static string ToArabic(this NotificationType type) => type switch
    {
        NotificationType.Info => "معلومات",
        NotificationType.Success => "نجاح",
        NotificationType.Warning => "تحذير",
        NotificationType.Error => "خطأ",
        NotificationType.Payment => "دفع",
        NotificationType.Complaint => "شكوى",
        NotificationType.RoomAssignment => "تعيين غرفة",
        NotificationType.Announcement => "إعلان",
        NotificationType.Reminder => "تذكير",
        NotificationType.System => "نظام",
        _ => type.ToString()
    };
    
    public static string GetIconClass(this NotificationType type) => type switch
    {
        NotificationType.Info => "fa-info-circle",
        NotificationType.Success => "fa-check-circle",
        NotificationType.Warning => "fa-exclamation-triangle",
        NotificationType.Error => "fa-times-circle",
        NotificationType.Payment => "fa-money-bill",
        NotificationType.Complaint => "fa-comment-dots",
        NotificationType.RoomAssignment => "fa-door-open",
        NotificationType.Announcement => "fa-bullhorn",
        NotificationType.Reminder => "fa-bell",
        NotificationType.System => "fa-cog",
        _ => "fa-bell"
    };
}