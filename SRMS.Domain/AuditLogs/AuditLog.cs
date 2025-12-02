using SRMS.Domain.AuditLogs.Enums;

namespace SRMS.Domain.AuditLogs;

/// <summary>
/// AuditLog - سجل التدقيق لتتبع كل العمليات في النظام
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    
    // Who (من قام بالعملية)
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    
    // What (ماذا حدث)
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    
    // When (متى)
    public DateTime Timestamp { get; set; }
    
    // How (كيف)
    public AuditAction AuditAction { get; set; }
    
    // Changes (التغييرات)
    public string? OldValues { get; set; }  // JSON
    public string? NewValues { get; set; }  // JSON
    
    // Where (من أين)
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Additional Info
    public string? AdditionalInfo { get; set; }
}