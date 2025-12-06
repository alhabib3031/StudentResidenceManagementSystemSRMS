using MediatR;

namespace SRMS.Application.Complaints.ResolveComplaint;

public record ResolveComplaintCommand(Guid ComplaintId, string Resolution) : IRequest<bool>;