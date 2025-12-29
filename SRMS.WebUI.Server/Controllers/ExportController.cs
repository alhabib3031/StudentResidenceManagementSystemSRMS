using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRMS.Application.AuditLogs.Interfaces;

namespace SRMS.WebUI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperRoot")]
    public class ExportController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public ExportController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> ExportAuditLogs()
        {
            var logs = await _auditService.GetAllAsync();
            
            var sb = new StringBuilder();
            sb.AppendLine("Timestamp,User,Action,Entity,Details,IP Address");

            foreach (var log in logs)
            {
                sb.AppendLine(
                    $"{log.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                    $"{EscapeCsv(log.UserName)}," +
                    $"{EscapeCsv(log.Action)}," +
                    $"{EscapeCsv(log.EntityName)}," +
                    $"{EscapeCsv(log.AdditionalInfo)}," +
                    $"{EscapeCsv(log.IpAddress)}");
            }

            var fileName = $"AuditLogs_{DateTime.Now:yyyyMMddHHmmss}.csv";
            var fileBytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(fileBytes, "text/csv", fileName);
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
