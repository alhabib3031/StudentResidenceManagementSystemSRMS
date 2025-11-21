namespace SRMS.Domain.AuditLogs.Enums;

/// <summary>
/// AuditAction Enum - Enhanced with all system operations
/// </summary>
public enum AuditAction
{
    // ══════════════════════════════════════════════
    // CRUD Operations
    // ══════════════════════════════════════════════
    Create = 1,         // إنشاء
    Read = 2,           // قراءة
    Update = 3,         // تحديث
    Delete = 4,         // حذف
    Restore = 5,        // استعادة (من Soft Delete)
    
    // ══════════════════════════════════════════════
    // Authentication & Authorization
    // ══════════════════════════════════════════════
    Login = 10,                 // تسجيل دخول
    Logout = 11,                // تسجيل خروج
    LoginFailed = 12,           // فشل تسجيل الدخول
    PasswordChanged = 13,       // تغيير كلمة المرور
    PasswordReset = 14,         // إعادة تعيين كلمة المرور
    EmailVerified = 15,         // تأكيد البريد الإلكتروني
    TwoFactorEnabled = 16,      // تفعيل المصادقة الثنائية
    TwoFactorDisabled = 17,     // تعطيل المصادقة الثنائية
    AccountLocked = 18,         // قفل الحساب
    AccountUnlocked = 19,       // فتح الحساب
    
    // ══════════════════════════════════════════════
    // Role & Permission Management
    // ══════════════════════════════════════════════
    RoleAssigned = 20,          // تعيين دور
    RoleRemoved = 21,           // إزالة دور
    PermissionGranted = 22,     // منح صلاحية
    PermissionRevoked = 23,     // إلغاء صلاحية
    
    // ══════════════════════════════════════════════
    // Data Operations
    // ══════════════════════════════════════════════
    Export = 30,        // تصدير
    Import = 31,        // استيراد
    Download = 32,      // تحميل
    Upload = 33,        // رفع
    Print = 34,         // طباعة
    
    // ══════════════════════════════════════════════
    // Student Operations
    // ══════════════════════════════════════════════
    StudentRegistered = 40,     // تسجيل طالب جديد
    StudentActivated = 41,      // تفعيل طالب
    StudentSuspended = 42,      // إيقاف طالب
    StudentGraduated = 43,      // تخرج طالب
    StudentWithdrawn = 44,      // انسحاب طالب
    RoomAssigned = 45,          // تعيين غرفة
    RoomUnassigned = 46,        // إلغاء تعيين غرفة
    
    // ══════════════════════════════════════════════
    // Payment Operations
    // ══════════════════════════════════════════════
    PaymentCreated = 50,        // إنشاء دفعة
    PaymentPaid = 51,           // دفع
    PaymentCancelled = 52,      // إلغاء دفعة
    PaymentRefunded = 53,       // استرداد
    PaymentOverdue = 54,        // دفعة متأخرة
    
    // ══════════════════════════════════════════════
    // Complaint Operations
    // ══════════════════════════════════════════════
    ComplaintSubmitted = 60,    // تقديم شكوى
    ComplaintAssigned = 61,     // تعيين شكوى
    ComplaintResolved = 62,     // حل شكوى
    ComplaintClosed = 63,       // إغلاق شكوى
    ComplaintReopened = 64,     // إعادة فتح شكوى
    
    // ══════════════════════════════════════════════
    // System Operations
    // ══════════════════════════════════════════════
    SystemStarted = 70,         // بدء النظام
    SystemShutdown = 71,        // إيقاف النظام
    ConfigurationChanged = 72,  // تغيير الإعدادات
    BackupCreated = 73,         // إنشاء نسخة احتياطية
    BackupRestored = 74,        // استعادة نسخة احتياطية
    DatabaseMigration = 75,     // ترحيل قاعدة البيانات
    
    // ══════════════════════════════════════════════
    // Notification Operations
    // ══════════════════════════════════════════════
    NotificationSent = 80,      // إرسال إشعار
    EmailSent = 81,             // إرسال بريد إلكتروني
    SMSSent = 82,               // إرسال رسالة نصية
    
    // ══════════════════════════════════════════════
    // Security Operations
    // ══════════════════════════════════════════════
    UnauthorizedAccess = 90,    // محاولة وصول غير مصرح
    SuspiciousActivity = 91,    // نشاط مشبوه
    SecurityAlertTriggered = 92,// تنبيه أمني
    
    // ══════════════════════════════════════════════
    // Error & Failure
    // ══════════════════════════════════════════════
    Error = 98,         // خطأ
    Failure = 99        // فشل
}