using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Notifications.DTOs;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Notifications;
using SRMS.Domain.Notifications.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Notifications.CreateNotification;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    private readonly IRepositories<Notification> _notificationRepository;
    private readonly IEmailService _emailService;
    private readonly ISMSService _smsService;

    public CreateNotificationCommandHandler(
        IRepositories<Notification> notificationRepository,
        IEmailService emailService,
        ISMSService smsService)
    {
        _notificationRepository = notificationRepository;
        _emailService = emailService;
        _smsService = smsService;
    }

    public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.Notification.UserId,
            UserEmail = request.Notification.UserEmail,
            UserPhone = request.Notification.UserPhone,
            Title = request.Notification.Title,
            Message = request.Notification.Message,
            Type = request.Notification.Type,
            Priority = request.Notification.Priority,
            SendEmail = request.Notification.SendEmail,
            SendSMS = request.Notification.SendSMS,
            SendInApp = request.Notification.SendInApp,
            Status = NotificationStatus.Pending,
            RelatedEntityType = request.Notification.RelatedEntityType,
            RelatedEntityId = request.Notification.RelatedEntityId,
            ActionUrl = request.Notification.ActionUrl,
            MetadataJson = request.Notification.MetadataJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _notificationRepository.CreateAsync(notification);

        // Send notifications asynchronously (fire and forget or use background job)
        _ = Task.Run(async () =>
        {
            try
            {
                bool emailSent = false;
                bool smsSent = false;

                if (created.SendEmail && !string.IsNullOrEmpty(created.UserEmail))
                {
                    emailSent = await _emailService.SendEmailAsync(
                        created.UserEmail,
                        created.Title,
                        created.Message,
                        isHtml: true
                    );
                }

                if (created.SendSMS && !string.IsNullOrEmpty(created.UserPhone))
                {
                    smsSent = await _smsService.SendSMSAsync(created.UserPhone, created.Message);
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
        }, cancellationToken);

        return new NotificationDto
        {
            Id = created.Id,
            Title = created.Title,
            Message = created.Message,
            Type = created.Type,
            Priority = created.Priority,
            Status = created.Status,
            IsRead = created.IsRead,
            CreatedAt = created.CreatedAt,
            SentAt = created.SentAt,
            ReadAt = created.ReadAt,
            ActionUrl = created.ActionUrl
        };
    }
}