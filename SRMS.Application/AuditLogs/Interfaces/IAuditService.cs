using SRMS.Domain.AuditLogs;
using SRMS.Domain.AuditLogs.Enums;

namespace SRMS.Application.AuditLogs.Interfaces;

public interface IAuditService
{
    // ═══════════════════════════════════════════════════════════
    // Core Logging Methods
    // ═══════════════════════════════════════════════════════════
    
    /// <summary>
    /// تسجيل حدث عام
    /// </summary>
    Task LogAsync(
        AuditAction action,
        string entityName,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? additionalInfo = null);

    // ═══════════════════════════════════════════════════════════
    // Specialized Logging Methods
    // ═══════════════════════════════════════════════════════════
    
    /// <summary>
    /// تسجيل محاولة تسجيل دخول
    /// </summary>
    Task LogLoginAttemptAsync(string email, bool success, string? reason = null);
    
    /// <summary>
    /// تسجيل عملية CRUD
    /// </summary>
    Task LogCrudAsync<T>(
        AuditAction action,
        T? oldEntity = null,
        T? newEntity = null,
        string? additionalInfo = null) where T : class;
    
    /// <summary>
    /// تسجيل تغيير دور المستخدم
    /// </summary>
    Task LogRoleChangeAsync(Guid userId, string role, bool isAdded);
    
    /// <summary>
    /// تسجيل عملية تصدير
    /// </summary>
    Task LogExportAsync(string exportType, int recordCount);
    
    /// <summary>
    /// تسجيل تغيير حالة الطالب
    /// </summary>
    Task LogStudentStatusChangeAsync(
        Guid studentId,
        string oldStatus,
        string newStatus,
        string reason);
    
    /// <summary>
    /// تسجيل عملية دفع
    /// </summary>
    Task LogPaymentAsync(
        Guid paymentId,
        string status,
        decimal amount,
        string? additionalInfo = null);
    
    /// <summary>
    /// تسجيل محاولة وصول غير مصرح
    /// </summary>
    Task LogUnauthorizedAccessAsync(string resource, string attemptedAction);

    // ═══════════════════════════════════════════════════════════
    // Query Methods
    // ═══════════════════════════════════════════════════════════
    
    Task<List<AuditLog>> GetAllAsync();
    Task<List<AuditLog>> GetTodayLogsUtcAsync();
    Task<List<AuditLog>> GetUserLogsAsync(Guid userId, int take = 100);
    Task<List<AuditLog>> GetEntityLogsAsync(string entityName, string entityId, int take = 100);
    Task<Dictionary<string, int>> GetActionStatisticsAsync(DateTime? fromDate = null);
}