using SRMS.Domain.AuditLogs;
using SRMS.Domain.AuditLogs.Enums;

namespace SRMS.Application.AuditLogs.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        AuditAction action,
        string entityName,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? additionalInfo = null);

    Task<List<AuditLog>> GetAllAsync();
    Task<List<AuditLog>> GetTodayLogsUtcAsync();

}