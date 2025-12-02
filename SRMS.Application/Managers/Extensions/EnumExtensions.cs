using SRMS.Domain.Managers.Enums;

namespace SRMS.Application.Managers.Extensions;

public static class EnumExtensions
{
    // ========== Manager Status ==========
    public static string ToArabic(this ManagerStatus status) => status switch
    {
        ManagerStatus.Active => "نشط",
        ManagerStatus.OnLeave => "في إجازة",
        ManagerStatus.Suspended => "معلق",
        ManagerStatus.Terminated => "تم إنهاء الخدمة",
        _ => status.ToString()
    };
}