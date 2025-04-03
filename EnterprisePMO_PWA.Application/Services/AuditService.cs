using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Services;
using Newtonsoft.Json;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Service for logging auditable actions in the system.
    /// </summary>
    public class AuditService
    {
        private readonly SupabaseClient _supabaseClient;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            SupabaseClient supabaseClient,
            ILogger<AuditService> logger)
        {
            _supabaseClient = supabaseClient;
            _logger = logger;
        }

        /// <summary>
        /// Logs an action asynchronously - updated to return a Task to support awaiting
        /// </summary>
        /// <param name="username">The username of the user performing the action</param>
        /// <param name="action">The action being performed</param>
        /// <param name="entityName">The name of the entity being affected</param>
        /// <param name="changeSummary">A summary of changes made</param>
        public Task LogAction(string username, string action, string entityName, string changeSummary)
        {
            // Just call the async version directly since it returns a Task
            return LogActionAsync(username, action, entityName, changeSummary);
        }

        /// <summary>
        /// Logs an auditable action asynchronously.
        /// </summary>
        /// <param name="username">The username of the user performing the action</param>
        /// <param name="action">The action being performed</param>
        /// <param name="entityName">The name of the entity being affected</param>
        /// <param name="changeSummary">A summary of changes made</param>
        public async Task LogActionAsync(string username, string action, string entityName, string changeSummary)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = entityName,
                    Action = action,
                    ChangeSummary = changeSummary,
                    Username = username,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "Unknown"
                };

                await _supabaseClient.Database.InsertAsync<AuditLog>(
                    "audit_logs",
                    auditLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit action");
            }
        }

        /// <summary>
        /// Logs an action with an entity ID.
        /// </summary>
        /// <param name="entityName">The name of the entity being affected</param>
        /// <param name="entityId">The ID of the entity being affected</param>
        /// <param name="action">The action being performed</param>
        /// <param name="changeSummary">A summary of changes made</param>
        public async Task LogActionAsync(string entityName, Guid entityId, string action, string changeSummary)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    ChangeSummary = changeSummary,
                    Username = "System",
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "Unknown"
                };

                await _supabaseClient.Database.InsertAsync<AuditLog>(
                    "audit_logs",
                    auditLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging audit action");
            }
        }

        /// <summary>
        /// Creates a change summary by comparing an original and modified entity.
        /// </summary>
        public string CreateChangeSummary<T>(T original, T modified)
        {
            var originalJson = JsonConvert.SerializeObject(original);
            var modifiedJson = JsonConvert.SerializeObject(modified);
            
            // In a real implementation, you would do a more detailed comparison
            // to extract exactly what properties changed
            return $"Changed from {originalJson} to {modifiedJson}";
        }
    }
}