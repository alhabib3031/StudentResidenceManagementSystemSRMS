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

    public ResolveComplaintCommandHandler(IRepositories<Complaint> complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<bool> Handle(ResolveComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _complaintRepository.GetByIdAsync(request.ComplaintId);

        if (complaint is null) return false;

        await _complaintRepository.UpdateAsync(complaint);

        return true;
    }
}