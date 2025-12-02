namespace SRMS.Domain.Complaints.Enums;

public enum ComplaintStatus
{
    Open = 0,           // مفتوحة
    InProgress = 1,     // قيد المعالجة
    Resolved = 2,       // تم الحل
    Closed = 3,         // مغلقة
    Cancelled = 4,      // ملغاة
    Reopened = 5        // تم إعادة الفتح
}