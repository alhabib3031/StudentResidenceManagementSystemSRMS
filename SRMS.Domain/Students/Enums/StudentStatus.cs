namespace SRMS.Domain.Students.Enums;

/// <summary>
/// StudentStatus Enum
/// </summary>
public enum StudentStatus
{
    EmailVerificationPending = 0, // في انتظار التحقق من البريد الإلكتروني
    ProfileCompletionPending = 1, // في انتظار إكمال الملف الشخصي
    ManagerApprovalPending = 2,   // في انتظار مراجعة المدير
    Active = 3,                   // نشط
    Suspended = 4,      // معلق
    Graduated = 5,      // تخرج
    Withdrawn = 6,      // انسحب
    Expelled = 7        // مفصول
}