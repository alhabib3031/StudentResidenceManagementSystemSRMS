using SRMS.Domain.Students;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Complaints;
﻿using MediatR;

namespace SRMS.Application.Complaints.AssignComplaint;

public record AssignComplaintCommand(Guid ComplaintId, Guid AssignToManagerId) : IRequest<bool>;