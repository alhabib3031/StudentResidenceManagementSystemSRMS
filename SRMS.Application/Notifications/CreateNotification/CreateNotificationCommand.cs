using MediatR;
using SRMS.Application.Notifications.DTOs;

namespace SRMS.Application.Notifications.CreateNotification;

public class CreateNotificationCommand : IRequest<NotificationDto>
{
    public CreateNotificationDto Notification { get; set; } = new();
}