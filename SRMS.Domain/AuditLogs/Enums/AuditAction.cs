namespace SRMS.Domain.AuditLogs.Enums;

/// <summary>
/// AuditAction Enum
/// </summary>
public enum AuditAction
{
    Create = 1,     // إنشاء
    Update = 2,     // تحديث
    Delete = 3,     // حذف
    View = 4,       // عرض
    Login = 5,      // تسجيل دخول
    Logout = 6,     // تسجيل خروج
    Export = 7,     // تصدير
    Import = 8      // استيراد
}