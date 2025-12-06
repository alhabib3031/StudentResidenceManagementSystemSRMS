using MediatR;
using SRMS.Application.Complaints.DTOs;

namespace SRMS.Application.Complaints.GetStudentComplaints;

/// <summary>
/// Query للحصول على شكاوى طالب معين
/// </summary>
public record GetStudentComplaintsQuery(Guid StudentId) : IRequest<List<ComplaintDto>>;
