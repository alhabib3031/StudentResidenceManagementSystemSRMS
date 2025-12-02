using SRMS.Domain.Notifications.Enums;

namespace SRMS.Application.Notifications.DTOs;

public class NotificationDetailsDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public NotificationPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool SendEmail { get; set; }
    public bool SendSMS { get; set; }
    public bool SendInApp { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}