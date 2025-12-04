using Mapster;
using MediatR;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Application.Complaints.DTOs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Complaints;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Complaints.CreateComplaint;

public class CreateComplaintCommandHandler : IRequestHandler<CreateComplaintCommand, ComplaintDto>
{
    private readonly IRepositories<Complaint> _complaintRepository;

    public CreateComplaintCommandHandler(IRepositories<Complaint> complaintRepository)
    {
        _complaintRepository = complaintRepository;
    }

    public async Task<ComplaintDto> Handle(CreateComplaintCommand request, CancellationToken cancellationToken)
    {
        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            StudentId = request.Complaint.StudentId,
            Title = request.Complaint.Title,
            Description = request.Complaint.Description,
            Category = request.Complaint.Category,
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

        // return new ComplaintDto
        // {
        //     Id = created.Id,
        //     ComplaintNumber = created.ComplaintNumber,
        //     Title = created.Title,
        //     Category = created.Category,
        //     Priority = created.Priority,
        //     Status = created.Status,
        //     StudentId = created.StudentId,
        //     StudentName = created.Student?.FullName ?? "",
        //     CreatedAt = created.CreatedAt,
        //     IsResolved = created.Status == ComplaintStatus.Resolved
        // };
        return complaint.Adapt<ComplaintDto>();
    }
}