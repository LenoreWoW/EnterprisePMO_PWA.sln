using System;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Enumerates the possible global roles for users.
    /// </summary>
    public enum RoleType
    {
        ProjectManager,
        SubPMO,
        MainPMO,
        DepartmentDirector,
        Executive,
        Admin
    }

    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; } // Unique identifier

        public string Username { get; set; } = string.Empty; // Email address

        public RoleType Role { get; set; } // Global role

        public Guid? DepartmentId { get; set; } // Optional FK to department
        public Department? Department { get; set; } // Navigation property
        
        /// <summary>
        /// The user's ID in the Supabase authentication system
        /// </summary>
        public string? SupabaseId { get; set; }
    }
}