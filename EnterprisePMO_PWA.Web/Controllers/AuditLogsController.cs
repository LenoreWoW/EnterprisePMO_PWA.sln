using System;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Web.Controllers
{
    /// <summary>
    /// API controller for accessing audit logs.
    /// </summary>
    [ApiController]
    [Route("api/auditlogs")]
    [Authorize(Roles = "Admin,MainPMO")]
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets audit logs for a specific entity.
        /// </summary>
        [HttpGet("entity/{entityName}/{entityId}")]
        public async Task<IActionResult> GetEntityAuditLogs(string entityName, Guid entityId)
        {
            var logs = await _context.AuditLogs
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new
                {
                    a.Id,
                    a.Action,
                    a.ChangeSummary,
                    UserName = a.Username, // Use the Username property directly
                    a.Timestamp,
                    a.IpAddress
                })
                .ToListAsync();

            return Ok(logs);
        }

        /// <summary>
        /// Gets recent audit logs with optional filtering.
        /// </summary>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentAuditLogs(
            [FromQuery] string? entityName = null,
            [FromQuery] string? action = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] int pageSize = 50)
        {
            var query = _context.AuditLogs.AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(entityName))
            {
                query = query.Where(a => a.EntityName == entityName);
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.Action == action);
            }

            if (userId.HasValue)
            {
                query = query.Where(a => a.UserId == userId.Value);
            }

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.EntityName,
                    a.EntityId,
                    a.Action,
                    a.ChangeSummary,
                    UserName = a.Username, // Use the Username property directly
                    a.Timestamp,
                    a.IpAddress
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}