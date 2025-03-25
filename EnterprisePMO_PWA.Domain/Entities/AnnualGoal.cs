using System;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents an annual goal used for aligning projects with long-term objectives.
    /// </summary>
    public class AnnualGoal
    {
        public Guid Id { get; set; } // Primary key

        public string Name { get; set; } = string.Empty; // Name of the goal

        public string Description { get; set; } = string.Empty; // Detailed description

        public Guid? StrategicGoalId { get; set; } // Optional FK to a strategic goal

        public StrategicGoal? StrategicGoal { get; set; } // Navigation property
    }
}
