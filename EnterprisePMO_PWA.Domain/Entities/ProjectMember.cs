using System;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a user's assignment to a project with a specific role.
    /// </summary>
    public class ProjectMember
    {
        public Guid Id { get; set; } // Unique membership identifier

        public Guid ProjectId { get; set; } // FK to project
        public Project? Project { get; set; } // Navigation property

        public Guid UserId { get; set; } // FK to user
        public User? User { get; set; } // Navigation property

        public Guid RoleId { get; set; } // FK to assigned role
        public Role? Role { get; set; } // Navigation property
    }
}
