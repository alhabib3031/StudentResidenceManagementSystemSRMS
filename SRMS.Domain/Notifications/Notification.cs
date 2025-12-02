using SRMS.Domain.Abstractions;
using SRMS.Domain.Notifications.Enums;

namespace SRMS.Domain.Notifications;

public class Notification : Entity
{
    // Recipient
    public Guid? UserId { get; set; }  // null = system-wide notification
    public string? UserEmail { get; set; }
    public string? UserPhone { get; set; }
    
    // Content
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    
    // Channels
    public bool SendEmail { get; set; }
    public bool SendSMS { get; set; }
    public bool SendInApp { get; set; }
    
    // Status
    public NotificationStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsRead { get; set; }
    
    // Metadata
    public string? RelatedEntityType { get; set; }  // "Payment", "Complaint", etc.
    public Guid? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }  // URL to navigate to
    public string? MetadataJson { get; set; }  // Additional data as JSON
    
    // Retry Logic
    public int RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public string? ErrorMessage { get; set; }
}