using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Complaints;
﻿using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Complaints.DTOs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Complaints.UpdateComplaint;

public class UpdateComplaintCommandHandler : IRequestHandler<UpdateComplaintCommand, ComplaintDto?>
{
    private readonly IRepositories<Complaint> _complaintRepository;
    private readonly IAuditService _audit;

    public UpdateComplaintCommandHandler(IRepositories<Complaint> complaintRepository, IAuditService audit)
    {
        _complaintRepository = complaintRepository;
        _audit = audit;
    }

    public async Task<ComplaintDto?> Handle(UpdateComplaintCommand request, CancellationToken cancellationToken)
    {
        var existing = await _complaintRepository.GetByIdAsync(request.Complaint.Id);

        if (existing == null)
        {
            await _audit.LogAsync(
                AuditAction.Failure,
                "Complaint",
                request.Complaint.Id.ToString(),
                additionalInfo: "Attempted to update non-existent complaint"
            );
            return null;
        }

        var oldValues = new
        {
            existing.Title,
            existing.Description,
            existing.ComplaintTypeId,
            existing.Priority,
            existing.Status,
            existing.AssignedTo,
            existing.Resolution
        };

        existing.Title = request.Complaint.Title;
        existing.Description = request.Complaint.Description;
        existing.ComplaintTypeId = request.Complaint.ComplaintTypeId;
        existing.Priority = request.Complaint.Priority;
        existing.Status = request.Complaint.Status;
        existing.AssignedTo = request.Complaint.AssignedTo;
        existing.Resolution = request.Complaint.Resolution;
        existing.UpdatesJson = request.Complaint.UpdatesJson;

        // If status changed to Resolved
        if (existing.Status == ComplaintStatus.Resolved && existing.ResolvedAt == null)
        {
            existing.ResolvedAt = DateTime.UtcNow;
        }

        // If assigned to someone
        if (request.Complaint.AssignedTo.HasValue && existing.AssignedAt == null)
        {
            existing.AssignedAt = DateTime.UtcNow;
        }

        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _complaintRepository.UpdateAsync(existing);

        var newValues = new
        {
            updated.Title,
            updated.Description,
            updated.ComplaintTypeId,
            updated.Priority,
            updated.Status,
            updated.AssignedTo,
            updated.Resolution
        };

        // ✅ Log complaint update
        await _audit.LogCrudAsync(
            action: AuditAction.Update,
            oldEntity: oldValues,
            newEntity: newValues,
            additionalInfo: $"Complaint updated: {updated.ComplaintNumber} - {updated.Title}"
        );

        // ✅ Log specific status changes
        if (oldValues.Status != newValues.Status)
        {
            var statusAction = newValues.Status switch
            {
                ComplaintStatus.Resolved => AuditAction.ComplaintResolved,
                ComplaintStatus.Closed => AuditAction.ComplaintClosed,
                ComplaintStatus.Reopened => AuditAction.ComplaintReopened,
                _ => AuditAction.Update
            };

            await _audit.LogAsync(
                action: statusAction,
                entityName: "Complaint",
                entityId: updated.Id.ToString(),
                oldValues: new { Status = oldValues.Status },
                newValues: new { Status = newValues.Status },
                additionalInfo: $"Complaint status changed from {oldValues.Status} to {newValues.Status}: {updated.ComplaintNumber}"
            );
        }

        return new ComplaintDto
        {
            Id = updated.Id,
            ComplaintNumber = updated.ComplaintNumber,
            Title = updated.Title,
            ComplaintType = updated.ComplaintType.Name,
            Priority = updated.Priority,
            Status = updated.Status,
            ReservationId = updated.ReservationId,
            CreatedAt = updated.CreatedAt,
            IsResolved = updated.Status == ComplaintStatus.Resolved
        };
    }
}