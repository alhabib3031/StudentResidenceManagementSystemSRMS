namespace SRMS.Domain.Complaints.Enums;

public enum ComplaintCategory
{
    Maintenance = 1,        // صيانة
    Cleanliness = 2,        // نظافة
    Noise = 3,              // ضوضاء
    Security = 4,           // أمن
    RoommateIssue = 5,      // مشكلة مع زميل الغرفة
    Facilities = 6,         // المرافق
    Management = 7,         // الإدارة
    Internet = 8,           // الإنترنت
    Utilities = 9,          // المرافق (كهرباء، ماء)
    Other = 99              // أخرى
}