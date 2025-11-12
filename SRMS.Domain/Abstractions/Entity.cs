namespace SRMS.Domain.Abstractions;

/// <summary>
/// Base Entity Class - يحتوي على الخصائص المشتركة لكل الكيانات
/// يوفر Audit Trail تلقائي
/// </summary>
public abstract class Entity
{
    public Guid Id { get; set; }
    
    // Audit Properties
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // Optional: Audit Fields (يمكن إضافتها لاحقاً)
    // public string? CreatedBy { get; set; }
    // public string? UpdatedBy { get; set; }
    // public string? LastModifiedByIp { get; set; }
}