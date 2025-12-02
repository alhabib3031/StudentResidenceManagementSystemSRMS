using System.Drawing;
using SRMS.Domain.AuditLogs.Enums;

namespace SRMS.Application.AuditLogs.Extensions;

/// <summary>
/// Extension methods for AuditAction
/// </summary>
public static class Extensions
{
    public static string ToArabic(this AuditAction action) => action switch
    {
        AuditAction.Create => "إنشاء",
        AuditAction.Read => "قراءة",
        AuditAction.Update => "تحديث",
        AuditAction.Delete => "حذف",
        AuditAction.Restore => "استعادة",
        
        AuditAction.Login => "تسجيل دخول",
        AuditAction.Logout => "تسجيل خروج",
        AuditAction.LoginFailed => "فشل تسجيل الدخول",
        AuditAction.PasswordChanged => "تغيير كلمة المرور",
        AuditAction.PasswordReset => "إعادة تعيين كلمة المرور",
        AuditAction.EmailVerified => "تأكيد البريد الإلكتروني",
        
        AuditAction.RoleAssigned => "تعيين دور",
        AuditAction.RoleRemoved => "إزالة دور",
        AuditAction.PermissionGranted => "منح صلاحية",
        AuditAction.PermissionRevoked => "إلغاء صلاحية",
        
        AuditAction.Export => "تصدير",
        AuditAction.Import => "استيراد",
        AuditAction.Download => "تحميل",
        AuditAction.Upload => "رفع",
        
        AuditAction.StudentRegistered => "تسجيل طالب",
        AuditAction.RoomAssigned => "تعيين غرفة",
        
        AuditAction.PaymentPaid => "دفع",
        AuditAction.ComplaintSubmitted => "تقديم شكوى",
        
        AuditAction.Error => "خطأ",
        AuditAction.Failure => "فشل",
        
        _ => action.ToString()
    };
    
    public static string ToEnglish(this AuditAction action) => action switch
    {
        AuditAction.Create => "Create",
        AuditAction.Read => "Read",
        AuditAction.Update => "Update",
        AuditAction.Delete => "Delete",
        AuditAction.Restore => "Restore",
        
        AuditAction.Login => "Login",
        AuditAction.Logout => "Logout",
        AuditAction.LoginFailed => "Login Failed",
        
        AuditAction.StudentRegistered => "Student Registered",
        AuditAction.RoomAssigned => "Room Assigned",
        
        AuditAction.PaymentPaid => "Payment Paid",
        AuditAction.ComplaintSubmitted => "Complaint Submitted",
        
        AuditAction.Error => "Error",
        AuditAction.Failure => "Failure",
        
        _ => action.ToString()
    };
    
}