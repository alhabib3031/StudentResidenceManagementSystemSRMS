using SRMS.Domain.Students;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Complaints;
﻿using MediatR;
using SRMS.Application.Complaints.DTOs;

namespace SRMS.Application.Complaints.UpdateComplaint;

public class UpdateComplaintCommand : IRequest<ComplaintDto?>
{
    public UpdateComplaintDto Complaint { get; set; } = new();
}