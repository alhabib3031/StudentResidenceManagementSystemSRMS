using SRMS.Application.Notifications.DTOs;
using SRMS.Domain.Notifications.Enums;

namespace SRMS.Application.Notifications.Interfaces;

public interface IInAppNotificationService
{
    Task<bool> CreateNotificationAsync(Guid userId, string title, string message, NotificationType type);
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int skip = 0, int take = 20);
    Task<int> GetUnreadCountAsync(Guid userId);
}