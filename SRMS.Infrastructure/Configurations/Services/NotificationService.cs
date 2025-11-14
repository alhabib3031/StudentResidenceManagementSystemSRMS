using SRMS.Application.Notifications.DTOs;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Domain.Notifications;
using SRMS.Domain.Notifications.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Infrastructure.Configurations.Services;

public class NotificationService : INotificationService
{
    private readonly IRepositories<Notification> _notificationRepository;
    private readonly IEmailService _emailService;
    private readonly ISMSService _smsService;
    
    public NotificationService(
        IRepositories<Notification> notificationRepository,
        IEmailService emailService,
        ISMSService smsService)
    {
        _notificationRepository = notificationRepository;
        _emailService = emailService;
        _smsService = smsService;
    }
    
    public async Task<bool> SendNotificationAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            UserEmail = dto.UserEmail,
            UserPhone = dto.UserPhone,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            Priority = dto.Priority,
            SendEmail = dto.SendEmail,
            SendSMS = dto.SendSMS,
            SendInApp = dto.SendInApp,
            Status = NotificationStatus.Pending,
            RelatedEntityType = dto.RelatedEntityType,
            RelatedEntityId = dto.RelatedEntityId,
            ActionUrl = dto.ActionUrl,
            MetadataJson = dto.MetadataJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        var created = await _notificationRepository.CreateAsync(notification);
        
        // Send notifications asynchronously
        _ = Task.Run(async () =>
        {
            try
            {
                if (created.SendEmail && !string.IsNullOrEmpty(created.UserEmail))
                {
                    await _emailService.SendEmailAsync(
                        created.UserEmail,
                        created.Title,
                        created.Message,
                        isHtml: true
                    );
                }
                
                if (created.SendSMS && !string.IsNullOrEmpty(created.UserPhone))
                {
                    await _smsService.SendSMSAsync(created.UserPhone, created.Message);
                }
                
                created.Status = NotificationStatus.Sent;
                created.SentAt = DateTime.UtcNow;
                await _notificationRepository.UpdateAsync(created);
            }
            catch (Exception ex)
            {
                created.Status = NotificationStatus.Failed;
                created.ErrorMessage = ex.Message;
                created.RetryCount++;
                created.LastRetryAt = DateTime.UtcNow;
                await _notificationRepository.UpdateAsync(created);
            }
        });
        
        return true;
    }
    
    public async Task<bool> SendBulkNotificationsAsync(List<CreateNotificationDto> notifications)
    {
        var tasks = notifications.Select(n => SendNotificationAsync(n));
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }
    
    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        
        if (notification == null || notification.UserId != userId)
            return false;
        
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.Status = NotificationStatus.Read;
        
        await _notificationRepository.UpdateAsync(notification);
        
        return true;
    }
    
    public async Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _notificationRepository
            .FindAsync(n => n.UserId == userId && !n.IsRead);
        
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.Status = NotificationStatus.Read;
            await _notificationRepository.UpdateAsync(notification);
        }
        
        return true;
    }
    
    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var notifications = await _notificationRepository
            .FindAsync(n => n.UserId == userId && !n.IsRead);
        
        return notifications.Count();
    }
    
    public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int skip = 0, int take = 20)
    {
        var notifications = await _notificationRepository
            .FindAsync(n => n.UserId == userId);
        
        return notifications
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                Priority = n.Priority,
                Status = n.Status,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                SentAt = n.SentAt,
                ReadAt = n.ReadAt,
                ActionUrl = n.ActionUrl
            })
            .ToList();
    }
}