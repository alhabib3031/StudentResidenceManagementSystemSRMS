using SRMS.Domain.Students;
using SRMS.Domain.Complaints;
using MediatR;
using SRMS.Application.Complaints.DTOs;
using SRMS.Domain.Repositories;

namespace SRMS.Application.Complaints.GetAllComplaints;

public class GetAllComplaintsQueryHandler : IRequestHandler<GetAllComplaintsQuery, List<ComplaintDto>>
{
    private readonly IRepositories<Complaint> _complaintRepository;
    private readonly IRepositories<Student> _studentRepository;

    public GetAllComplaintsQueryHandler(
        IRepositories<Complaint> complaintRepository,
        IRepositories<Student> studentRepository)
    {
        _complaintRepository = complaintRepository;
        _studentRepository = studentRepository;
    }

    public async Task<List<ComplaintDto>> Handle(GetAllComplaintsQuery request, CancellationToken cancellationToken)
    {
        // 1. Fetch complaints (Still heavy if table is huge, better to use pagination in future)
        var complaints = (await _complaintRepository.GetAllAsync())
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        if (!complaints.Any())
            return new List<ComplaintDto>();

        // 2. Extract distinct Student IDs to fetch only necessary students
        var studentIds = complaints
            .Select(c => c.ReservationId)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        // 3. Fetch ONLY the related students (Performance Optimization)
        // Instead of fetching ALL students, we filter by IDs on the database side (if repo supports it)
        var students = await _studentRepository.FindAsync(s => studentIds.Contains(s.Id));

        var studentDict = students.ToDictionary(s => s.Id, s => s.FullName);

        var result = complaints
            .Select(c => new ComplaintDto
            {
                Id = c.Id,
                ComplaintNumber = c.ComplaintNumber,
                Title = c.Title,
                ComplaintType = c.ComplaintType.Name,
                Priority = c.Priority,
                Status = c.Status,
                ReservationId = c.ReservationId,
                StudentName = studentDict.GetValueOrDefault(c.ReservationId, "Unknown"),
                CreatedAt = c.CreatedAt,
                IsResolved = c.Status == Domain.Complaints.Enums.ComplaintStatus.Resolved
            })
            .ToList();

        return result;
    }
}
