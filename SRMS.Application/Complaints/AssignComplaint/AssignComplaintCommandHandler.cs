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

        if (complaint is null) return false;

        await _complaintRepository.UpdateAsync(complaint);

        return true;
    }
}