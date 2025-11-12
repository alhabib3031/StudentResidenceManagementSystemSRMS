using MediatR;

namespace SRMS.Application.Notifications.MarkAsRead;

public class MarkNotificationAsReadCommand : IRequest<bool>
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; }
}