using System;
using System.Collections.Generic;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a department within the organization.
    /// </summary>
    [Table("departments")]
    public class Department : BaseModel
    {
        [Column("id")]
        public Guid Id { get; set; } // Unique identifier

        [Column("name")]
        public string Name { get; set; } = string.Empty; // Department name
        
        [Column("description")]
        public string Description { get; set; } = string.Empty; // Department description

        // Navigation properties
        public ICollection<User>? Users { get; set; }
        public ICollection<Project>? Projects { get; set; }
    }
}