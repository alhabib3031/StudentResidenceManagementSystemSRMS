using SRMS.Application.Notifications.DTOs;

namespace SRMS.Application.Notifications.Interfaces;

// ============================================================
// NOTIFICATION SERVICES INTERFACES
// ============================================================

public interface INotificationService
{
    Task<bool> SendNotificationAsync(CreateNotificationDto notification);
    Task<bool> SendBulkNotificationsAsync(List<CreateNotificationDto> notifications);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task<bool> MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, int skip = 0, int take = 20);
}