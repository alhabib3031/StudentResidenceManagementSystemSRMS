using MediatR;

namespace SRMS.Application.Complaints.AssignComplaint;

public class AssignComplaintCommand : IRequest<bool>
{
    public Guid ComplaintId { get; set; }
    public Guid AssignToManagerId { get; set; }
}