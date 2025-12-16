using SRMS.Domain.Students;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Complaints;
﻿using MediatR;

namespace SRMS.Application.Complaints.ResolveComplaint;

public record ResolveComplaintCommand(Guid ComplaintId, string Resolution) : IRequest<bool>;