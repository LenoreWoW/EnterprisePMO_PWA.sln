using System;
using System.ComponentModel.DataAnnotations;

namespace EnterprisePMO_PWA.Domain.Entities
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; }
        
        public string EntityName { get; set; } = string.Empty;
        
        public Guid EntityId { get; set; }
        
        public string Action { get; set; } = string.Empty;
        
        public string ChangeSummary { get; set; } = string.Empty;
        
        public Guid UserId { get; set; }
        
        // Added for compatibility with the LogAction method
        public string Username { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; }
        
        public string IpAddress { get; set; } = string.Empty;
    }
}