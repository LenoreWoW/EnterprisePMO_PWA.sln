using System;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a strategic goal linked to projects.
    /// </summary>
    [Table("strategic_goals")]
    public class StrategicGoal : BaseModel
    {
        [Column("id")]
        public Guid Id { get; set; } // Unique identifier

        [Column("name")]
        public string Name { get; set; } = string.Empty; // Name of the goal

        [Column("description")]
        public string Description { get; set; } = string.Empty; // Detailed description
    }
}
