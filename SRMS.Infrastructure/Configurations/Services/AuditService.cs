using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Domain.AuditLogs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Identity;

namespace SRMS.Infrastructure.Configurations.Services;

// <summary>
/// Enhanced AuditService with comprehensive logging
/// </summary>
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _http;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuditService(
        ApplicationDbContext db,
        IHttpContextAccessor http,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _http = http;
        _userManager = userManager;
    }

    /// <summary>
    /// تسجيل حدث في نظام التدقيق
    /// </summary>
    public async Task LogAsync(
        AuditAction action,
        string entityName,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? additionalInfo = null)
    {
        try
        {
            var httpContext = _http.HttpContext;
            var currentUser = await GetCurrentUserAsync();

            var log = new AuditLog
            {
                Id = Guid.NewGuid(),

                // What happened
                Action = GetActionDescription(action, entityName),
                AuditAction = action,
                EntityName = entityName,
                EntityId = entityId,

                // Who did it
                UserId = currentUser?.Id.ToString(),
                UserName = currentUser?.Email ?? "System",

                // When
                Timestamp = DateTime.UtcNow,

                // Changes
                OldValues = oldValues != null ? SerializeObject(oldValues) : null,
                NewValues = newValues != null ? SerializeObject(newValues) : null,

                // Context
                IpAddress = GetIpAddress(httpContext),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),

                // Additional
                AdditionalInfo = additionalInfo
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error but don't throw - audit failure shouldn't break the main operation
            Console.WriteLine($"Audit logging failed: {ex.Message}");
        }
    }

    /// <summary>
    /// تسجيل محاولة تسجيل دخول
    /// </summary>
    public async Task LogLoginAttemptAsync(string email, bool success, string? reason = null)
    {
        var action = success ? AuditAction.Login : AuditAction.LoginFailed;
        var info = success
            ? "Login successful"
            : $"Login failed: {reason ?? "Invalid credentials"}";

        await LogAsync(
            action: action,
            entityName: "Authentication",
            entityId: email,
            additionalInfo: info
        );
    }

    /// <summary>
    /// تسجيل عملية CRUD
    /// </summary>
    public async Task LogCrudAsync<T>(
        AuditAction action,
        T? oldEntity = null,
        T? newEntity = null,
        string? additionalInfo = null) where T : class
    {
        var entityName = typeof(T).Name;
        string? entityId = null;

        // Try to get entity ID
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null)
        {
            entityId = (action == AuditAction.Create ? newEntity : oldEntity) != null
                ? idProperty.GetValue(action == AuditAction.Create ? newEntity : oldEntity)?.ToString()
                : null;
        }

        await LogAsync(
            action: action,
            entityName: entityName,
            entityId: entityId,
            oldValues: oldEntity,
            newValues: newEntity,
            additionalInfo: additionalInfo
        );
    }

    /// <summary>
    /// تسجيل تغيير دور المستخدم
    /// </summary>
    public async Task LogRoleChangeAsync(Guid userId, string role, bool isAdded)
    {
        var action = isAdded ? AuditAction.RoleAssigned : AuditAction.RoleRemoved;
        var info = isAdded ? $"Role '{role}' assigned" : $"Role '{role}' removed";

        await LogAsync(
            action: action,
            entityName: "UserRole",
            entityId: userId.ToString(),
            additionalInfo: info
        );
    }

    /// <summary>
    /// تسجيل عملية تصدير
    /// </summary>
    public async Task LogExportAsync(string exportType, int recordCount)
    {
        await LogAsync(
            action: AuditAction.Export,
            entityName: exportType,
            additionalInfo: $"Exported {recordCount} records"
        );
    }

    /// <summary>
    /// تسجيل تغيير حالة الطالب
    /// </summary>
    public async Task LogStudentStatusChangeAsync(
        Guid studentId,
        string oldStatus,
        string newStatus,
        string reason)
    {
        var action = newStatus switch
        {
            "Active" => AuditAction.StudentActivated,
            "Suspended" => AuditAction.StudentSuspended,
            "Graduated" => AuditAction.StudentGraduated,
            "Withdrawn" => AuditAction.StudentWithdrawn,
            _ => AuditAction.Update
        };

        await LogAsync(
            action: action,
            entityName: "Student",
            entityId: studentId.ToString(),
            oldValues: new { Status = oldStatus },
            newValues: new { Status = newStatus },
            additionalInfo: reason
        );
    }

    /// <summary>
    /// تسجيل عملية دفع
    /// </summary>
    public async Task LogPaymentAsync(
        Guid paymentId,
        string status,
        decimal amount,
        string? additionalInfo = null)
    {
        var action = status switch
        {
            "Paid" => AuditAction.PaymentPaid,
            "Cancelled" => AuditAction.PaymentCancelled,
            "Refunded" => AuditAction.PaymentRefunded,
            _ => AuditAction.PaymentCreated
        };

        await LogAsync(
            action: action,
            entityName: "Payment",
            entityId: paymentId.ToString(),
            newValues: new { Status = status, Amount = amount },
            additionalInfo: additionalInfo
        );
    }

    /// <summary>
    /// تسجيل محاولة وصول غير مصرح
    /// </summary>
    public async Task LogUnauthorizedAccessAsync(
        string resource,
        string attemptedAction)
    {
        await LogAsync(
            action: AuditAction.UnauthorizedAccess,
            entityName: resource,
            additionalInfo: $"Attempted: {attemptedAction}"
        );
    }

    public async Task<List<AuditLog>> GetAllAsync()
    {
        return await _db.AuditLogs
            .OrderByDescending(x => x.Timestamp)
            .Take(1000) // Limit for performance
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetTodayLogsUtcAsync()
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(1);

        return await _db.AuditLogs
            .Where(log => log.Timestamp >= start && log.Timestamp < end)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetUserLogsAsync(Guid userId, int take = 100)
    {
        return await _db.AuditLogs
            .Where(log => log.UserId == userId.ToString())
            .OrderByDescending(log => log.Timestamp)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetEntityLogsAsync(
        string entityName,
        string entityId,
        int take = 100)
    {
        return await _db.AuditLogs
            .Where(log => log.EntityName == entityName && log.EntityId == entityId)
            .OrderByDescending(log => log.Timestamp)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetActionStatisticsAsync(DateTime? fromDate = null)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(log => log.Timestamp >= fromDate.Value);

        return await query
            .GroupBy(log => log.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Action, x => x.Count);
    }

    // ═══════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var httpContext = _http.HttpContext;
        if (httpContext?.User == null)
            return null;

        return await _userManager.GetUserAsync(httpContext.User);
    }

    private string GetIpAddress(HttpContext? context)
    {
        if (context == null) return "Unknown";

        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetActionDescription(AuditAction action, string entityName)
    {
        return $"{action} {entityName}";
    }

    private string SerializeObject(object obj)
    {
        try
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            });
        }
        catch
        {
            return obj.ToString() ?? "Serialization failed";
        }
    }
}