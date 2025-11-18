using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SRMS.Application.AuditLogs.Interfaces;
using SRMS.Domain.AuditLogs;
using SRMS.Domain.AuditLogs.Enums;
using SRMS.Domain.Identity;

namespace SRMS.Infrastructure.Configurations.Services;

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

    public async Task LogAsync(
        AuditAction action,
        string entityName,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        string? additionalInfo = null)
    {
        var httpContext = _http.HttpContext;

        var currentUser = await _userManager.GetUserAsync(
            httpContext?.User ?? new System.Security.Claims.ClaimsPrincipal());

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            Action = action.ToString(),
            AuditAction = action,
            EntityName = entityName,
            EntityId = entityId,

            // من
            UserId = currentUser?.Id.ToString(),
            UserName = currentUser?.Email,

            // متى
            Timestamp = DateTime.Now,

            // كيف
            OldValues = oldValues != null ? JsonConvert.SerializeObject(oldValues) : null,
            NewValues = newValues != null ? JsonConvert.SerializeObject(newValues) : null,

            // من أي IP و UserAgent
            IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request?.Headers["User-Agent"],

            AdditionalInfo = additionalInfo
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
    
    public async Task<List<AuditLog>> GetAllAsync()
    {
        return await _db.AuditLogs
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }
    
    public async Task<List<AuditLog>> GetTodayLogsUtcAsync()
    {
        var start = DateTime.Now.Date;
        var end = start.AddDays(1);

        return await _db.AuditLogs
            .Where(log => log.Timestamp >= start && log.Timestamp < end)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

}