using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Notifications.DTOs;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Notifications;
using SRMS.Domain.Notifications.Enums;
using SRMS.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using SRMS.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace SRMS.Infrastructure.Configurations.Services;

public class NotificationService : INotificationService
{
    private readonly IRepositories<Notification> _notificationRepository;
    private readonly IEmailService _emailService;
    private readonly ISMSService _smsService;
    private readonly IAuditService _audit;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(
        IRepositories<Notification> notificationRepository,
        IEmailService emailService,
        ISMSService smsService,
        IAuditService audit,
        UserManager<ApplicationUser> userManager)
    {
        _notificationRepository = notificationRepository;
        _emailService = emailService;
        _smsService = smsService;
        _audit = audit;
        _userManager = userManager;
    }

    public async Task<bool> SendNotificationAsync(CreateNotificationDto dto)
    {
        // ✅ Auto-resolve UserId from Email if not provided
        if (dto.UserId == null && !string.IsNullOrEmpty(dto.UserEmail))
        {
            var user = await _userManager.FindByEmailAsync(dto.UserEmail);
            if (user != null)
            {
                dto.UserId = user.Id;
            }
        }

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

        // ✅ Log notification
        await _audit.LogAsync(
            action: AuditAction.NotificationSent,
            entityName: "Notification",
            entityId: created.Id.ToString(),
            additionalInfo: $"Notification sent: {created.Title}"
        );

        // Send notifications asynchronously
        _ = Task.Run(async () =>
        {
            try
            {
                if (created.SendEmail && !string.IsNullOrEmpty(created.UserEmail))
                {
                    var sent = await _emailService.SendEmailAsync(
                        created.UserEmail,
                        created.Title,
                        created.Message,
                        isHtml: true
                    );

                    if (sent)
                    {
                        await _audit.LogAsync(
                            action: AuditAction.EmailSent,
                            entityName: "Notification",
                            entityId: created.Id.ToString(),
                            additionalInfo: $"Email sent: {created.UserEmail}"
                        );
                    }
                }

                if (created.SendSMS && !string.IsNullOrEmpty(created.UserPhone))
                {
                    var sent = await _smsService.SendSMSAsync(created.UserPhone, created.Message);

                    if (sent)
                    {
                        await _audit.LogAsync(
                            action: AuditAction.SMSSent,
                            entityName: "Notification",
                            entityId: created.Id.ToString(),
                            additionalInfo: $"SMS sent: {created.UserPhone}"
                        );
                    }
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

                await _audit.LogAsync(
                    action: AuditAction.Failure,
                    entityName: "Notification",
                    entityId: created.Id.ToString(),
                    additionalInfo: $"Notification failed: {ex.Message}"
                );
            }
        });

        return true;
    }

    public async Task<bool> SendBulkNotificationsAsync(List<CreateNotificationDto> notifications)
    {
        var tasks = notifications.Select(n => SendNotificationAsync(n));
        var results = await Task.WhenAll(tasks);

        // ✅ Log bulk notification
        await _audit.LogAsync(
            action: AuditAction.NotificationSent,
            entityName: "Notification",
            additionalInfo: $"Bulk notifications sent: {notifications.Count} recipients"
        );

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

        var count = 0;
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.Status = NotificationStatus.Read;
            await _notificationRepository.UpdateAsync(notification);
            count++;
        }

        if (count > 0)
        {
            // ✅ Log bulk mark as read
            await _audit.LogAsync(
                action: AuditAction.Update,
                entityName: "Notification",
                additionalInfo: $"Marked {count} notifications as read for user {userId}"
            );
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


    public async Task<bool> SendNotificationToRoleAsync(string roleName, CreateNotificationDto notification)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        if (!users.Any()) return false;

        var notifications = users.Select(u => new CreateNotificationDto
        {
            UserId = u.Id,
            UserEmail = u.Email,
            UserPhone = u.PhoneNumber,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            Priority = notification.Priority,
            SendEmail = notification.SendEmail,
            SendSMS = notification.SendSMS,
            SendInApp = notification.SendInApp,
            RelatedEntityType = notification.RelatedEntityType,
            RelatedEntityId = notification.RelatedEntityId,
            ActionUrl = notification.ActionUrl,
            MetadataJson = notification.MetadataJson
        }).ToList();

        return await SendBulkNotificationsAsync(notifications);
    }

    public async Task<bool> SendNotificationToAllAsync(CreateNotificationDto notification)
    {
        var users = await _userManager.Users.ToListAsync();
        if (!users.Any()) return false;

        var notifications = users.Select(u => new CreateNotificationDto
        {
            UserId = u.Id,
            UserEmail = u.Email,
            UserPhone = u.PhoneNumber,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            Priority = notification.Priority,
            SendEmail = notification.SendEmail,
            SendSMS = notification.SendSMS,
            SendInApp = notification.SendInApp,
            RelatedEntityType = notification.RelatedEntityType,
            RelatedEntityId = notification.RelatedEntityId,
            ActionUrl = notification.ActionUrl,
            MetadataJson = notification.MetadataJson
        }).ToList();

        return await SendBulkNotificationsAsync(notifications);
    }
}