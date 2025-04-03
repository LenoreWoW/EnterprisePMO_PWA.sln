using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EnterprisePMO_PWA.Domain.Enums;
using TableAttribute = Supabase.Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Supabase.Postgrest.Attributes.ColumnAttribute;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a global role with hierarchical permissions.
    /// </summary>
    [Table("roles")]
    public class Role
    {
        [Column("id")]
        public Guid Id { get; set; } // Unique identifier

        [Column("name")]
        public string RoleName { get; set; } = string.Empty; // Role name

        [Column("description")]
        public string Description { get; set; } = string.Empty; // Role description
        
        [Column("type")]
        public UserRole Type { get; set; }
        
        public int HierarchyLevel { get; set; } // Position in role hierarchy (higher = more permissions)
        
        public bool CanManageProjects { get; set; } // Can create, edit, delete projects
        
        public bool CanManageUsers { get; set; } // Can manage users
        
        public bool CanApproveRequests { get; set; } // Can approve change requests
        
        public bool CanManageRoles { get; set; } // Can create, edit, delete roles
        
        public bool CanViewReports { get; set; } // Can view reports and analytics
        
        public bool CanViewAuditLogs { get; set; } // Can view audit logs
        
        // For Discord-like inheritance, a role inherits all permissions from lower roles
        public bool InheritsPermissions { get; set; } = true;

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation: Project memberships using this role.
        public ICollection<ProjectMember>? ProjectMembers { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}