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

    public MarkNotificationAsReadCommandHandler(IRepositories<Notification> notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);

        if (notification == null || notification.UserId != request.UserId) return false;

        if (notification.IsRead)
        {
            // Already read, no need to update
            return true;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        notification.Status = NotificationStatus.Read;

        await _notificationRepository.UpdateAsync(notification);

        return true;
    }
}