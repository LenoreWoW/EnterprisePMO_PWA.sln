using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Service for creating audit log entries to track user actions.
    /// </summary>
    public class AuditService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Logs an action performed on an entity by the current user.
        /// </summary>
        /// <param name="entityName">Name of the entity type (e.g., "Project", "ChangeRequest")</param>
        /// <param name="entityId">ID of the affected entity</param>
        /// <param name="action">Action performed (e.g., "Create", "Update", "Delete", "Approve")</param>
        /// <param name="changeSummary">Description of the changes made</param>
        /// <returns>The created audit log entry</returns>
        public async Task<AuditLog> LogActionAsync(string entityName, Guid entityId, string action, string changeSummary)
        {
            // Get the current user ID from the HttpContext
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new InvalidOperationException("User ID not found in claims");
            }

            // Get client IP address
            string ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                ChangeSummary = changeSummary,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return auditLog;
        }

        /// <summary>
        /// Creates a change summary by comparing original and updated objects.
        /// </summary>
        /// <param name="original">Original object state</param>
        /// <param name="updated">Updated object state</param>
        /// <returns>JSON string describing changes</returns>
        public string CreateChangeSummary(object original, object updated)
        {
            var originalProps = original.GetType().GetProperties();
            var changes = new System.Collections.Generic.Dictionary<string, object?[]>();

            foreach (var prop in originalProps)
            {
                // Skip navigation properties and collections
                if (prop.PropertyType.Namespace == "System.Collections.Generic" || 
                    prop.PropertyType.IsClass && prop.PropertyType.Namespace != "System")
                {
                    continue;
                }

                var originalValue = prop.GetValue(original);
                var updatedValue = prop.GetValue(updated);

                // Compare the values (handling null cases)
                bool valuesEqual = (originalValue == null && updatedValue == null) ||
                                  (originalValue != null && originalValue.Equals(updatedValue));

                if (!valuesEqual)
                {
                    changes[prop.Name] = new object?[] { originalValue, updatedValue };
                }
            }

            return JsonConvert.SerializeObject(changes);
        }
    }
}