using MediatR;
using SRMS.Application.Complaints.DTOs;
using SRMS.Domain.Complaints;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Complaints.UpdateComplaint;

public class UpdateComplaintCommandHandler : IRequestHandler<UpdateComplaintCommand, ComplaintDto?>
{
    private readonly IRepositories<Complaint> _complaintRepository;

    public UpdateComplaintCommandHandler(IRepositories<Complaint> complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<ComplaintDto?> Handle(UpdateComplaintCommand request, CancellationToken cancellationToken)
    {
        var existing = await _complaintRepository.GetByIdAsync(request.Complaint.Id);
        
        if (existing == null)
            return null;
        
        existing.Title = request.Complaint.Title;
        existing.Description = request.Complaint.Description;
        existing.Category = request.Complaint.Category;
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
        
        return new ComplaintDto
        {
            Id = updated.Id,
            ComplaintNumber = updated.ComplaintNumber,
            Title = updated.Title,
            Category = updated.Category,
            Priority = updated.Priority,
            Status = updated.Status,
            StudentId = updated.StudentId,
            StudentName = updated.Student?.FullName ?? "",
            CreatedAt = updated.CreatedAt,
            IsResolved = updated.Status == ComplaintStatus.Resolved
        };
    }
}