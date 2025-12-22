using SRMS.Domain.Students.Enums;
using SRMS.Domain.ValueObjects;
using SRMS.Domain.Common; // For Nationality

namespace SRMS.Application.SystemSettings.DTOs;

public record CreateFeesConfigurationDto(
    Guid NationalityId,
    StudyLevel StudyLevel,
    bool IsMonthly,
    decimal Amount,
    string Currency,
    string? Description
);

public record UpdateFeesConfigurationDto(
    Guid Id,
    Guid NationalityId,
    StudyLevel StudyLevel,
    bool IsMonthly,
    decimal Amount,
    string Currency,
    string? Description
);

public record FeesConfigurationDto(
    Guid Id,
    Guid NationalityId,
    string NationalityName, // Assuming we want to display the nationality name
    StudyLevel StudyLevel,
    string StudyLevelName, // Assuming we want to display the study level name
    bool IsMonthly,
    Money FeeAmount,
    string? Description
);