namespace SRMS.Domain.Abstractions;

public abstract class Entity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? DeletedBy { get; set; }
    // public string? LastModifiedBy { get; set; }
    // public string? LastModifiedByIp { get; set; }
    // public string? LastModifiedByDevice { get; set; }
    // public string? LastModifiedByBrowser { get; set; }
    // public string? LastModifiedByOperatingSystem { get; set; }
    // public string? LastModifiedByLocation { get; set; }
    
    
}