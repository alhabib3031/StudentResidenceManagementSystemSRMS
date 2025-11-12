using MediatR;
using SRMS.Domain.Complaints;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Complaints.AssignComplaint;

public class AssignComplaintCommandHandler : IRequestHandler<AssignComplaintCommand, bool>
{
    private readonly IRepositories<Complaint> _complaintRepository;

    public AssignComplaintCommandHandler(IRepositories<Complaint> complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<bool> Handle(AssignComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = await _complaintRepository.GetByIdAsync(request.ComplaintId);
        
        if (complaint == null)
            return false;
        
        complaint.AssignedTo = request.AssignToManagerId;
        complaint.AssignedAt = DateTime.UtcNow;
        complaint.Status = ComplaintStatus.Open;
        complaint.UpdatedAt = DateTime.UtcNow;
        
        await _complaintRepository.UpdateAsync(complaint);
        
        return true;
    }
}