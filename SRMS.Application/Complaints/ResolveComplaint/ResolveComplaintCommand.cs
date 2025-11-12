using MediatR;

namespace SRMS.Application.Complaints.ResolveComplaint;

public class ResolveComplaintCommand : IRequest<bool>
{
    public Guid ComplaintId { get; set; }
    public Guid ResolvedByManagerId { get; set; }
    public string Resolution { get; set; } = string.Empty;
}