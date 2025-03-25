using System;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a strategic goal linked to projects.
    /// </summary>
    public class StrategicGoal
    {
        public Guid Id { get; set; } // Unique identifier

        public string Name { get; set; } = string.Empty; // Name of the goal

        public string Description { get; set; } = string.Empty; // Detailed description
    }
}
