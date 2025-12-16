using SRMS.Domain.Students;
using SRMS.Domain.Complaints.Enums;
using SRMS.Domain.Complaints;
﻿using System.ComponentModel.DataAnnotations;

namespace SRMS.Application.Complaints.DTOs;

public class CreateComplaintDto
{
    [Required]
    public Guid ReservationId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid ComplaintTypeId { get; set; }

    public ComplaintPriority Priority { get; set; } = ComplaintPriority.Medium;

    public string? AttachmentsJson { get; set; }
}