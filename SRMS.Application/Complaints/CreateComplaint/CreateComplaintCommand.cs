using MediatR;
using SRMS.Application.Complaints.DTOs;

namespace SRMS.Application.Complaints.CreateComplaint;

public record CreateComplaintCommand(CreateComplaintDto Complaint) : IRequest<ComplaintDto>;