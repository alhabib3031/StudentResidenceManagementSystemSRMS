using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Complaints;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Complaints.AssignComplaint;

public class AssignComplaintCommandHandler : IRequestHandler<AssignComplaintCommand, bool>
{
    private readonly IRepositories<Complaint> _complaintRepository;
    private readonly IAuditService _audit;
    
    public AssignComplaintCommandHandler(IRepositories<Complaint> complaintRepository, IAuditService audit)
    {
        _complaintRepository = complaintRepository;
        _audit = audit;
    }

    public async Task<bool> Handle(AssignComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _complaintRepository.GetByIdAsync(request.ComplaintId);
        
        if (complaint == null)
            return false;
        
        var oldAssignee = complaint.AssignedTo;
        
        complaint.AssignedTo = request.AssignToManagerId;
        complaint.AssignedAt = DateTime.UtcNow;
        complaint.Status = ComplaintStatus.Open;
        complaint.UpdatedAt = DateTime.UtcNow;
        
        await _complaintRepository.UpdateAsync(complaint);
        
        // ✅ Log complaint assignment
        await _audit.LogAsync(
            action: AuditAction.ComplaintAssigned,
            entityName: "Complaint",
            entityId: complaint.Id.ToString(),
            oldValues: new { AssignedTo = oldAssignee },
            newValues: new { AssignedTo = complaint.AssignedTo },
            additionalInfo: $"Complaint assigned to manager {request.AssignToManagerId}"
        );
        
        return true;
    }
}