using System;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents an annual goal used for aligning projects with long-term objectives.
    /// </summary>
    [Table("annual_goals")]
    public class AnnualGoal : BaseModel
    {
        [Column("id")]
        public Guid Id { get; set; } // Primary key

        [Column("name")]
        public string Name { get; set; } = string.Empty; // Name of the goal

        [Column("description")]
        public string Description { get; set; } = string.Empty; // Detailed description

        [Column("strategic_goal_id")]
        public Guid? StrategicGoalId { get; set; } // Optional FK to a strategic goal

        public StrategicGoal? StrategicGoal { get; set; } // Navigation property
    }
}
