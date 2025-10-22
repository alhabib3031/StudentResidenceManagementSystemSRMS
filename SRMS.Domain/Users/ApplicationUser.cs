using Microsoft.AspNetCore.Identity;

namespace SRMS.Domain.Users;

public class ApplicationUser : IdentityUser
{
    // Properties الموروثة من IdentityUser:
    // - Id (string)
    // - UserName
    // - Email
    // - PasswordHash
    // - PhoneNumber
    // - EmailConfirmed
    // - TwoFactorEnabled
    // - LockoutEnabled
    // - AccessFailedCount
    
    // نضيف Properties مخصصة:
    
    /// <summary>
    /// الاسم الأول للمستخدم
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// الاسم الأخير للمستخدم
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// تاريخ إنشاء الحساب
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// آخر تسجيل دخول
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// الاسم الكامل - Computed Property
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
    
    /// <summary>
    /// صورة الملف الشخصي (URL)
    /// </summary>
    public string? ProfilePictureUrl { get; set; }
}