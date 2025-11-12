namespace SRMS.Domain.Students.Enums;

/// <summary>
/// StudentStatus Enum
/// </summary>
public enum StudentStatus
{
    Pending = 0,        // في انتظار المراجعة
    Active = 1,         // نشط
    Suspended = 2,      // معلق
    Graduated = 3,      // تخرج
    Withdrawn = 4,      // انسحب
    Expelled = 5        // مفصول
}