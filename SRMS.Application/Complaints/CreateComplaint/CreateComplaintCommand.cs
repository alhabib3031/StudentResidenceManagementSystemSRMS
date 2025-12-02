using MediatR;
using SRMS.Application.Complaints.DTOs;

namespace SRMS.Application.Complaints.CreateComplaint;

public class CreateComplaintCommand : IRequest<ComplaintDto>
{
    public CreateComplaintDto Complaint { get; set; } = new();
}