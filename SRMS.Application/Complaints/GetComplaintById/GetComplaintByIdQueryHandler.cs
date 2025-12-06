using MediatR;
using SRMS.Application.Complaints.DTOs;
using SRMS.Domain.Complaints;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;

namespace SRMS.Application.Complaints.GetComplaintById;

public class GetComplaintByIdQueryHandler : IRequestHandler<GetComplaintByIdQuery, ComplaintDto?>
{
    private readonly IRepositories<Complaint> _complaintRepository;
    private readonly IRepositories<Student> _studentRepository;

    public GetComplaintByIdQueryHandler(
        IRepositories<Complaint> complaintRepository,
        IRepositories<Student> studentRepository)
    {
        _complaintRepository = complaintRepository;
        _studentRepository = studentRepository;
    }

    public async Task<ComplaintDto?> Handle(GetComplaintByIdQuery request, CancellationToken cancellationToken)
    {
        var complaint = await _complaintRepository.GetByIdAsync(request.Id);

        if (complaint == null || complaint.IsDeleted)
            return null;

        string studentName = "Unknown";
        if (complaint.StudentId != Guid.Empty)
        {
            var student = await _studentRepository.GetByIdAsync(complaint.StudentId);
            if (student != null)
            {
                studentName = student.FullName;
            }
        }

        return new ComplaintDto
        {
            Id = complaint.Id,
            ComplaintNumber = complaint.ComplaintNumber,
            Title = complaint.Title,
            Description = complaint.Description,
            Category = complaint.Category,
            Priority = complaint.Priority,
            Status = complaint.Status,
            CreatedAt = complaint.CreatedAt,
            StudentId = complaint.StudentId,
            StudentName = studentName,
            IsResolved = complaint.Status == Domain.Complaints.Enums.ComplaintStatus.Resolved,

            // Map Resolution Details
            ResolutionNotes = complaint.Resolution, // Corrected property name mapping
            ResolvedAt = complaint.ResolvedAt,
            ResolvedByManagerId = complaint.ResolvedBy // Corrected property name mapping
        };
    }
}
