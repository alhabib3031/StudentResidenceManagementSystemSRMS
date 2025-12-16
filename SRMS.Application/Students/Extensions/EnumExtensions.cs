using SRMS.Domain.Students.Enums;
namespace SRMS.Application.Students.Extensions;

public static class EnumExtensions
{
    // ========== Student Status ==========
    public static string ToArabic(this StudentStatus status) => status switch
    {
        StudentStatus.EmailVerificationPending => "في انتظار المراجعة",
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
        StudentStatus.EmailVerificationPending => "badge-warning",
        StudentStatus.Suspended => "badge-danger",
        StudentStatus.Graduated => "badge-info",
        StudentStatus.Withdrawn => "badge-secondary",
        StudentStatus.Expelled => "badge-danger",
        _ => "badge-secondary"
    };
}