using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Managers.Enums;
using SRMS.Domain.Notifications.Enums;
using SRMS.Domain.Payments.Enums;
using SRMS.Domain.Rooms.Enums;
using SRMS.Domain.Students.Enums;

namespace SRMS.Domain.Extensions;

public static class EnumExtensions
{
    // ========== Student Status ==========
    public static string ToArabic(this StudentStatus status) => status switch
    {
        StudentStatus.Pending => "في انتظار المراجعة",
        StudentStatus.Active => "نشط",
        StudentStatus.Suspended => "معلق",
        StudentStatus.Graduated => "متخرج",
        StudentStatus.Withdrawn => "منسحب",
        StudentStatus.Expelled => "مفصول",
        _ => status.ToString()
    };
    
    public static string GetBadgeClass(this StudentStatus status) => status switch
    {
        StudentStatus.Active => "badge-success",
        StudentStatus.Pending => "badge-warning",
        StudentStatus.Suspended => "badge-danger",
        StudentStatus.Graduated => "badge-info",
        StudentStatus.Withdrawn => "badge-secondary",
        StudentStatus.Expelled => "badge-danger",
        _ => "badge-secondary"
    };
    
    // ========== Manager Status ==========
    public static string ToArabic(this ManagerStatus status) => status switch
    {
        ManagerStatus.Active => "نشط",
        ManagerStatus.OnLeave => "في إجازة",
        ManagerStatus.Suspended => "معلق",
        ManagerStatus.Terminated => "تم إنهاء الخدمة",
        _ => status.ToString()
    };
    
    // ========== Room Type ==========
    public static string ToArabic(this RoomType type) => type switch
    {
        RoomType.Single => "غرفة فردية",
        RoomType.Double => "غرفة مزدوجة",
        RoomType.Triple => "غرفة ثلاثية",
        RoomType.Quad => "غرفة رباعية",
        RoomType.Dormitory => "مهجع",
        RoomType.Suite => "جناح",
        _ => type.ToString()
    };
    
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
    
    // ========== Complaint Category ==========
    public static string ToArabic(this ComplaintCategory category) => category switch
    {
        ComplaintCategory.Maintenance => "صيانة",
        ComplaintCategory.Cleanliness => "نظافة",
        ComplaintCategory.Noise => "ضوضاء",
        ComplaintCategory.Security => "أمن",
        ComplaintCategory.RoommateIssue => "مشكلة مع زميل الغرفة",
        ComplaintCategory.Facilities => "المرافق",
        ComplaintCategory.Management => "الإدارة",
        ComplaintCategory.Internet => "الإنترنت",
        ComplaintCategory.Utilities => "المرافق العامة",
        ComplaintCategory.Other => "أخرى",
        _ => category.ToString()
    };
    
    // ========== Complaint Priority ==========
    public static string ToArabic(this ComplaintPriority priority) => priority switch
    {
        ComplaintPriority.Low => "منخفضة",
        ComplaintPriority.Medium => "متوسطة",
        ComplaintPriority.High => "عالية",
        ComplaintPriority.Critical => "حرجة",
        _ => priority.ToString()
    };
    
    public static string GetBadgeClass(this ComplaintPriority priority) => priority switch
    {
        ComplaintPriority.Low => "badge-secondary",
        ComplaintPriority.Medium => "badge-info",
        ComplaintPriority.High => "badge-warning",
        ComplaintPriority.Critical => "badge-danger",
        _ => "badge-secondary"
    };
    
    // ========== Complaint Status ==========
    public static string ToArabic(this ComplaintStatus status) => status switch
    {
        ComplaintStatus.Open => "مفتوحة",
        ComplaintStatus.InProgress => "قيد المعالجة",
        ComplaintStatus.Resolved => "تم الحل",
        ComplaintStatus.Closed => "مغلقة",
        ComplaintStatus.Cancelled => "ملغاة",
        ComplaintStatus.Reopened => "تم إعادة الفتح",
        _ => status.ToString()
    };
    
    public static string GetBadgeClass(this ComplaintStatus status) => status switch
    {
        ComplaintStatus.Open => "badge-warning",
        ComplaintStatus.InProgress => "badge-info",
        ComplaintStatus.Resolved => "badge-success",
        ComplaintStatus.Closed => "badge-secondary",
        ComplaintStatus.Cancelled => "badge-danger",
        ComplaintStatus.Reopened => "badge-warning",
        _ => "badge-secondary"
    };
    
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