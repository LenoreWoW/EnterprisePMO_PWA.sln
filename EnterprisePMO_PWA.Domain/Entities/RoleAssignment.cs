using System;
using Supabase.Postgrest.Attributes;

namespace EnterprisePMO_PWA.Domain.Entities
{
    [Table("role_assignments")]
    public class RoleAssignment : BaseEntityModel
    {
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("role_name")]
        public string RoleName { get; set; } = string.Empty;

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; }

        public RoleAssignment()
        {
            AssignedAt = DateTime.UtcNow;
        }
    }
} 