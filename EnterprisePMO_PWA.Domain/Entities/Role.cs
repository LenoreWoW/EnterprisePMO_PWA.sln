using System;
using System.Collections.Generic;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a global role with hierarchical permissions.
    /// </summary>
    public class Role
    {
        public Guid Id { get; set; } // Unique identifier

        public string RoleName { get; set; } = string.Empty; // Role name

        public string Description { get; set; } = string.Empty; // Role description
        
        public int HierarchyLevel { get; set; } // Position in role hierarchy (higher = more permissions)
        
        public bool CanManageProjects { get; set; } // Can create, edit, delete projects
        
        public bool CanManageUsers { get; set; } // Can manage users
        
        public bool CanApproveRequests { get; set; } // Can approve change requests
        
        public bool CanManageRoles { get; set; } // Can create, edit, delete roles
        
        public bool CanViewReports { get; set; } // Can view reports and analytics
        
        public bool CanViewAuditLogs { get; set; } // Can view audit logs
        
        // For Discord-like inheritance, a role inherits all permissions from lower roles
        public bool InheritsPermissions { get; set; } = true;

        // Navigation: Project memberships using this role.
        public ICollection<ProjectMember>? ProjectMembers { get; set; }
    }
}