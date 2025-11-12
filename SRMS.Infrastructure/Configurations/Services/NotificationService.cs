using SRMS.Application.Notifications.DTOs;
using SRMS.Application.Notifications.Interfaces;

namespace SRMS.Infrastructure.Configurations.Services;

public class NotificationService : INotificationService
{
    public Task<bool> SendNotificationAsync(CreateNotificationDto notification)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SendBulkNotificationsAsync(List<CreateNotificationDto> notifications)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MarkAllAsReadAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetUnreadCountAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int skip = 0, int take = 20)
    {
        throw new NotImplementedException();
    }
}