using SRMS.Domain.Students;
using SRMS.Domain.Complaints;
using MediatR;
using SRMS.Application.Complaints.DTOs;
using SRMS.Domain.Repositories;

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
        if (complaint.ReservationId != Guid.Empty)
        {
            var student = await _studentRepository.GetByIdAsync(complaint.ReservationId);
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
            ComplaintType = complaint.ComplaintType.Name,
            Priority = complaint.Priority,
            Status = complaint.Status,
            CreatedAt = complaint.CreatedAt,
            ReservationId = complaint.ReservationId,
            StudentName = studentName,
            IsResolved = complaint.Status == Domain.Complaints.Enums.ComplaintStatus.Resolved,

            // Map Resolution Details
            ResolutionNotes = complaint.Resolution, // Corrected property name mapping
            ResolvedAt = complaint.ResolvedAt,
            ResolvedByManagerId = complaint.ResolvedBy // Corrected property name mapping
        };
    }
}
