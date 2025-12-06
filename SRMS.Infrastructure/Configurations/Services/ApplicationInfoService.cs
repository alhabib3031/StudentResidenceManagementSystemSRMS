using SRMS.Application.Common.Interfaces;

namespace SRMS.Infrastructure.Configurations.Services;

/// <summary>
/// تنفيذ خدمة معلومات التطبيق - يُسجَّل كـ Singleton
/// Application Info Service Implementation - Registered as Singleton
/// </summary>
public class ApplicationInfoService : IApplicationInfoService
{
    /// <summary>
    /// وقت بدء التشغيل - يُحدَّد مرة واحدة عند إنشاء الخدمة
    /// </summary>
    public DateTime StartTimeUtc { get; } = DateTime.UtcNow;

    /// <summary>
    /// مدة التشغيل المتواصل منذ بدء التطبيق
    /// </summary>
    public TimeSpan Uptime => DateTime.UtcNow - StartTimeUtc;

    /// <summary>
    /// عدد أيام التشغيل المتواصل
    /// </summary>
    public int UptimeDays => (int)Uptime.TotalDays;

    /// <summary>
    /// الحصول على نص مُنسَّق لمدة التشغيل
    /// Format: "Xd Xh Xm" or "Xh Xm" or "Xm"
    /// </summary>
    public string GetFormattedUptime()
    {
        var uptime = Uptime;

        if (uptime.TotalDays >= 1)
        {
            return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
        }

        if (uptime.TotalHours >= 1)
        {
            return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
        }

        return $"{(int)uptime.TotalMinutes}m";
    }
}
