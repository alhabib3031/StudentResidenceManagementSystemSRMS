using SRMS.Domain.Complaints.Enums;

namespace SRMS.Application.Complaints.Extensions;

public static class EnumExtensions
{
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
}