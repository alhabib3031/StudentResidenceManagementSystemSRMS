using MediatR;
using SRMS.Application.Complaints.DTOs;
using SRMS.Domain.Complaints;
using SRMS.Domain.Repositories;
using SRMS.Domain.Students;

namespace SRMS.Application.Complaints.GetStudentComplaints;

public class GetStudentComplaintsQueryHandler : IRequestHandler<GetStudentComplaintsQuery, List<ComplaintDto>>
{
    private readonly IRepositories<Complaint> _complaintRepository;
    private readonly IRepositories<Student> _studentRepository;

    public GetStudentComplaintsQueryHandler(
        IRepositories<Complaint> complaintRepository,
        IRepositories<Student> studentRepository)
    {
        _complaintRepository = complaintRepository;
        _studentRepository = studentRepository;
    }

    public async Task<List<ComplaintDto>> Handle(GetStudentComplaintsQuery request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        var studentName = student?.FullName ?? "Unknown";

        var complaints = await _complaintRepository.FindAsync(c => c.StudentId == request.StudentId && !c.IsDeleted);

        var result = complaints
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ComplaintDto
            {
                Id = c.Id,
                ComplaintNumber = c.ComplaintNumber,
                Title = c.Title,
                Category = c.Category,
                Priority = c.Priority,
                Status = c.Status,
                StudentId = c.StudentId,
                StudentName = studentName,
                CreatedAt = c.CreatedAt,
                IsResolved = c.Status == Domain.Complaints.Enums.ComplaintStatus.Resolved
            })
            .ToList();

        return result;
    }
}
