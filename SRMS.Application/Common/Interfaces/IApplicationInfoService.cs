namespace SRMS.Application.Common.Interfaces;

/// <summary>
/// خدمة معلومات التطبيق - تتبع وقت بدء التشغيل ومدة التشغيل
/// Application Info Service - Tracks application start time and uptime
/// </summary>
public interface IApplicationInfoService
{
    /// <summary>
    /// وقت بدء تشغيل التطبيق (UTC)
    /// </summary>
    DateTime StartTimeUtc { get; }

    /// <summary>
    /// مدة تشغيل التطبيق منذ البداية
    /// </summary>
    TimeSpan Uptime { get; }

    /// <summary>
    /// عدد أيام التشغيل المتواصل
    /// </summary>
    int UptimeDays { get; }

    /// <summary>
    /// الحصول على نص مُنسَّق لمدة التشغيل (مثال: "5d 3h 20m")
    /// </summary>
    string GetFormattedUptime();
}
