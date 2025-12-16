using SRMS.Domain.Students;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Complaints;
using MediatR;
using SRMS.Application.Complaints.DTOs;

namespace SRMS.Application.Complaints.GetStudentComplaints;

/// <summary>
/// Query للحصول على شكاوى طالب معين
/// </summary>
public record GetStudentComplaintsQuery(Guid ReservationId) : IRequest<List<ComplaintDto>>;
