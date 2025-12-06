using MediatR;
using SRMS.Application.Complaints.DTOs;

namespace SRMS.Application.Complaints.GetComplaintById;

public record GetComplaintByIdQuery(Guid Id) : IRequest<ComplaintDto?>;
