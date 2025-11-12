using MediatR;
using SRMS.Domain.Complaints;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Complaints.ResolveComplaint;

public class ResolveComplaintCommandHandler : IRequestHandler<ResolveComplaintCommand, bool>
{
    private readonly IRepositories<Complaint> _complaintRepository;

    public ResolveComplaintCommandHandler(IRepositories<Complaint> complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<bool> Handle(ResolveComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _complaintRepository.GetByIdAsync(request.ComplaintId);
        
        if (complaint == null)
            return false;
        
        complaint.Status = ComplaintStatus.Resolved;
        complaint.Resolution = request.Resolution;
        complaint.ResolvedAt = DateTime.UtcNow;
        complaint.ResolvedBy = request.ResolvedByManagerId;
        complaint.UpdatedAt = DateTime.UtcNow;
        
        await _complaintRepository.UpdateAsync(complaint);
        
        return true;
    }
}