namespace SRMS.Domain.Managers.Enums;

/// <summary>
/// ManagerStatus Enum
/// </summary>
public enum ManagerStatus
{
    Active = 1,         // نشط
    OnLeave = 2,        // في إجازة
    Suspended = 3,      // معلق
    Terminated = 4      // تم إنهاء الخدمة
}