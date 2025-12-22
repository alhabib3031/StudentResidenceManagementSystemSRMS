using SRMS.Domain.Abstractions;
using SRMS.Domain.Students.Enums;
using SRMS.Domain.ValueObjects;
using SRMS.Domain.Common; // Added


namespace SRMS.Domain.SystemSettings;

/// <summary>
/// FeesConfiguration Entity - إعدادات الرسوم الديناميكية
/// </summary>
public class FeesConfiguration : Entity
{
    public Guid? NationalityId { get; set; }
    public Nationality? Nationality { get; set; }

    public StudyLevel StudyLevel { get; set; }

    public bool IsMonthly { get; set; } = true; // true = Monthly, false = One Time (Season)
    public Money FeeAmount { get; set; } = Money.Zero("SAR");

    public string? Description { get; set; }
}
