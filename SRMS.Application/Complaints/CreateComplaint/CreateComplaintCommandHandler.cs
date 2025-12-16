using SRMS.Domain.Students;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Complaints;
using Mapster;
using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Complaints.DTOs;
using SRMS.Application.Notifications.DTOs;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Notifications.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Complaints.CreateComplaint;

public class CreateComplaintCommandHandler : IRequestHandler<CreateComplaintCommand, ComplaintDto>
{
    private readonly IRepositories<Complaint> _complaintRepository;
    private readonly IAuditService _audit;
    private readonly INotificationService _notificationService;

    public CreateComplaintCommandHandler(
        IRepositories<Complaint> complaintRepository,
        IAuditService audit,
        INotificationService notificationService)
    {
        _complaintRepository = complaintRepository;
        _audit = audit;
        _notificationService = notificationService;
    }

    public async Task<ComplaintDto> Handle(CreateComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            ReservationId = request.Complaint.ReservationId,
            Title = request.Complaint.Title,
            Description = request.Complaint.Description,
            ComplaintTypeId = request.Complaint.ComplaintTypeId,
            Priority = request.Complaint.Priority,
            Status = ComplaintStatus.Open,
            ComplaintNumber = $"CMP-{DateTime.UtcNow:yyyyMMddHHmmss}",
            AttachmentsJson = request.Complaint.AttachmentsJson,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false
        };

        var created = await _complaintRepository.CreateAsync(complaint);

        // ✅ Log complaint submission
        await _audit.LogAsync(
            action: AuditAction.ComplaintSubmitted,
            entityName: "Complaint",
            entityId: created.Id.ToString(),
            newValues: new
            {
                created.ComplaintNumber,
                created.Title,
                created.ComplaintTypeId,
                created.Priority,
                created.ReservationId
            },
            additionalInfo: $"Complaint submitted: {created.ComplaintNumber} - {created.Title} (Priority: {created.Priority}, ComplaintTypeId: {created.ComplaintTypeId})"
        );

        // ✅ Send notification to Managers
        var notification = new CreateNotificationDto
        {
            Title = "New Complaint Submitted",
            Message = $"New complaint from student [{created.ComplaintNumber}]: {created.Title}",
            Type = NotificationType.Complaint,
            Priority = MapPriority(created.Priority),
            SendInApp = true,
            SendEmail = true, // Optional: Send email as well
            RelatedEntityType = "Complaint",
            RelatedEntityId = created.Id,
            ActionUrl = $"/complaints/{created.Id}" // Link to manager's view
        };

        // Send to "Manager" Role
        await _notificationService.SendNotificationToRoleAsync("Manager", notification);

        return complaint.Adapt<ComplaintDto>();
    }

    private NotificationPriority MapPriority(ComplaintPriority priority) => priority switch
    {
        ComplaintPriority.Critical => NotificationPriority.High,
        ComplaintPriority.High => NotificationPriority.Low,
        _ => NotificationPriority.Low
    };
}