using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Complaints;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Complaints.ResolveComplaint;

public class ResolveComplaintCommandHandler : IRequestHandler<ResolveComplaintCommand, bool>
{
    private readonly IRepositories<Complaint> _complaintRepository;
    private readonly IAuditService _audit;
    
    public ResolveComplaintCommandHandler(IRepositories<Complaint> complaintRepository, IAuditService audit)
    {
        _complaintRepository = complaintRepository;
        _audit = audit;
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
        complaint.ResolvedBy = request.ResolvedByManagerId;
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
                complaint.ResolvedBy,
                complaint.ResolvedAt
            },
            additionalInfo: $"Complaint {complaint.ComplaintNumber} resolved by manager {request.ResolvedByManagerId} - Resolution: {request.Resolution}"
        );
        
        return true;
    }
}