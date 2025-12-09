using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Notifications;
using SRMS.Domain.Notifications.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Notifications.MarkAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly IRepositories<Notification> _notificationRepository;
    private readonly IAuditService _audit;

    public MarkNotificationAsReadCommandHandler(IRepositories<Notification> notificationRepository, IAuditService audit)
    {
        _notificationRepository = notificationRepository;
        _audit = audit;
    }

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);

        if (notification == null || notification.UserId != request.UserId)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Notification",
                request.NotificationId.ToString(),
                additionalInfo: $"Attempted to mark non-existent or unauthorized notification as read by user {request.UserId}"
            );
            return false;
        }

        if (notification.IsRead)
        {
            // Already read, no need to update
            return true;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.Status = NotificationStatus.Read;

        await _notificationRepository.UpdateAsync(notification);

        // ✅ Log notification marked as read
        await _audit.LogAsync(
            action: AuditAction.Read,
            entityName: "Notification",
            entityId: notification.Id.ToString(),
            additionalInfo: $"Notification marked as read: {notification.Title}"
        );

        return true;
    }
}