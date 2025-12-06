using MediatR;

namespace SRMS.Application.Complaints.AssignComplaint;

public record AssignComplaintCommand(Guid ComplaintId, Guid AssignToManagerId) : IRequest<bool>;