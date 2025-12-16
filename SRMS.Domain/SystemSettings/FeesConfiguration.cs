using SRMS.Domain.Abstractions;
using SRMS.Domain.ValueObjects;

namespace SRMS.Domain.SystemSettings;

/// <summary>
/// FeesConfiguration Entity - إعدادات الرسوم الديناميكية
/// </summary>
public class FeesConfiguration : Entity
{
    public string Nationality { get; set; } = string.Empty; // e.g., "Libya", "Palestine", "Other"
    public string StudyType { get; set; } = string.Empty; // e.g., "Bachelor", "Master", "PhD"
    public bool IsMonthly { get; set; } = true; // Monthly or for the whole stay
    public Money FeeAmount { get; set; } = Money.Zero("SAR"); // Default to 0 SAR

    // Admin control properties
    public new bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    public static FeesConfiguration CreateDefault(string nationality, string studyType, decimal amount, string currency, bool isMonthly, string description)
    {
        return new FeesConfiguration
        {
            Nationality = nationality,
            StudyType = studyType,
            FeeAmount = Money.Create(amount, currency),
            IsMonthly = isMonthly,
            Description = description
        };
    }
}
