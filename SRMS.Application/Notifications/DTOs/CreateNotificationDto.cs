using System.ComponentModel.DataAnnotations;
using SRMS.Domain.Notifications.Enums;

namespace SRMS.Application.Notifications.DTOs;

public class CreateNotificationDto
{
    public Guid? UserId { get; set; }
    
    [EmailAddress]
    public string? UserEmail { get; set; }
    
    [Phone]
    public string? UserPhone { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    
    public bool SendEmail { get; set; } = true;
    public bool SendSMS { get; set; } = false;
    public bool SendInApp { get; set; } = true;
    
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    public string? MetadataJson { get; set; }
}