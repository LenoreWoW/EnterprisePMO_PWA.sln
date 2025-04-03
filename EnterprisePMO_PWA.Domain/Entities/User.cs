using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EnterprisePMO_PWA.Domain.Enums;
using Supabase.Postgrest.Models;
using TableAttribute = Supabase.Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Supabase.Postgrest.Attributes.ColumnAttribute;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    [Table("users")]
    public class User : BaseModel
    {
        [Column("id")]
        public Guid Id { get; set; } // Unique identifier

        [Column("username")]
        public string Username { get; set; } = string.Empty; // Email address

        [Column("role")]
        public UserRole Role { get; set; } // Global role

        [Column("department_id")]
        public Guid DepartmentId { get; set; } // Optional FK to department
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } // Navigation property
        
        /// <summary>
        /// The user's ID in the Supabase authentication system
        /// </summary>
        [Column("supabase_id")]
        public string SupabaseId { get; set; }

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("salt")]
        public string Salt { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ProjectMember> ProjectMemberships { get; set; }
        public virtual ICollection<ProjectTask> AssignedTasks { get; set; }
    }
}