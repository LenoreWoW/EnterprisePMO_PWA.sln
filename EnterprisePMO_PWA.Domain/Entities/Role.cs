using System;
using System.Collections.Generic;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a global role (e.g., Admin, Moderator, Member).
    /// </summary>
    public class Role
    {
        public Guid Id { get; set; } // Unique identifier

        public string RoleName { get; set; } = string.Empty; // Role name

        public string Description { get; set; } = string.Empty; // Role description

        // Navigation: Project memberships using this role.
        public ICollection<ProjectMember>? ProjectMembers { get; set; }
    }
}
