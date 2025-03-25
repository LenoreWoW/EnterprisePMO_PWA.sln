using System;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents an audit trail entry for tracking changes to entities.
    /// </summary>
    public class AuditLog
    {
        public Guid Id { get; set; }
        
        public string EntityName { get; set; } = string.Empty;
        
        public Guid EntityId { get; set; }
        
        public string Action { get; set; } = string.Empty;
        
        public string ChangeSummary { get; set; } = string.Empty;
        
        public Guid UserId { get; set; }
        
        public User? User { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public string IpAddress { get; set; } = string.Empty;
    }
}