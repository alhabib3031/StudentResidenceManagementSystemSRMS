using SRMS.Domain.Students;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Complaints;
﻿using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Notifications.DTOs;
using SRMS.Application.Notifications.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Notifications.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Complaints.ResolveComplaint;

public class ResolveComplaintCommandHandler : IRequestHandler<ResolveComplaintCommand, bool>
{
    private readonly IRepositories<Complaint> _complaintRepository;
    private readonly IRepositories<Student> _studentRepository;
    private readonly IAuditService _audit;
    private readonly INotificationService _notificationService;

    public ResolveComplaintCommandHandler(
        IRepositories<Complaint> complaintRepository,
        IRepositories<Student> studentRepository,
        IAuditService audit,
        INotificationService notificationService)
    {
        _complaintRepository = complaintRepository;
        _studentRepository = studentRepository;
        _audit = audit;
        _notificationService = notificationService;
    }

    public async Task<bool> Handle(ResolveComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _complaintRepository.GetByIdAsync(request.ComplaintId);

        if (complaint == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Complaint",
                request.ComplaintId.ToString(),
                additionalInfo: "Attempted to resolve non-existent complaint"
            );
            return false;
        }

        var oldStatus = complaint.Status;

        complaint.Status = ComplaintStatus.Resolved;
        complaint.Resolution = request.Resolution;
        complaint.ResolvedAt = DateTime.UtcNow;
        complaint.UpdatedAt = DateTime.UtcNow;

        await _complaintRepository.UpdateAsync(complaint);

        // ✅ Log complaint resolution
        await _audit.LogAsync(
            action: AuditAction.ComplaintResolved,
            entityName: "Complaint",
            entityId: complaint.Id.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new
            {
                Status = ComplaintStatus.Resolved,
                complaint.Resolution,
                complaint.ResolvedAt
            },
            additionalInfo: $"Complaint {complaint.ComplaintNumber} resolved - Resolution: {request.Resolution}"
        );

        // ✅ Send notification to Student
        if (complaint.ReservationId != Guid.Empty)
        {
            var student = await _studentRepository.GetByIdAsync(complaint.ReservationId);
            if (student != null && student.Email != null)
            {
                var notification = new CreateNotificationDto
                {
                    UserEmail = student.Email.Value, // Send via Email mapping
                    Title = $"Complaint Resolved: {complaint.ComplaintNumber}",
                    Message = $"Your complaint '{complaint.Title}' has been resolved Check your email for details.\n\nManager Notes: {request.Resolution}",
                    Type = NotificationType.System,
                    Priority = NotificationPriority.Low,
                    SendInApp = true,
                    SendEmail = true,
                    RelatedEntityType = "Complaint",
                    RelatedEntityId = complaint.Id,
                    ActionUrl = $"/student/complaints" // Link to student's complaints page
                };

                await _notificationService.SendNotificationAsync(notification);
            }
        }

        return true;
    }
}