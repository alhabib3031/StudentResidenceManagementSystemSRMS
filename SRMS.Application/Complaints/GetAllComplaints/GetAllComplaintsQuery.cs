using MediatR;
using SRMS.Application.Complaints.DTOs;

namespace SRMS.Application.Complaints.GetAllComplaints;

/// <summary>
/// Query للحصول على جميع الشكاوى (للمدراء)
/// </summary>
public record GetAllComplaintsQuery : IRequest<List<ComplaintDto>>;
