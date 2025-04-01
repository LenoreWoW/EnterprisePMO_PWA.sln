using System;
using System.Collections.Generic;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a department within the organization.
    /// </summary>
    public class Department
    {
        public Guid Id { get; set; } // Unique identifier

        public string Name { get; set; } = string.Empty; // Department name
        
        public string Description { get; set; } = string.Empty; // Department description

        // Navigation properties
        public ICollection<User>? Users { get; set; }
        public ICollection<Project>? Projects { get; set; }
    }
}