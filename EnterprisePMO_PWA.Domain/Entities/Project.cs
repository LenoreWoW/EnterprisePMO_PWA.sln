using System;
using System.Collections.Generic;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Enumerates the possible project statuses.
    /// </summary>
    public enum ProjectStatus
    {
        Proposed,
        Active,
        Rejected,
        Completed
    }

    /// <summary>
    /// Enumerates color codes for visual status indication.
    /// </summary>
    public enum StatusColor
    {
        Green,   // > 30 days remaining
        Blue,    // (Optional, for ongoing work)
        Yellow,  // 10-30 days remaining
        Red      // < 10 days remaining
    }

    /// <summary>
    /// Represents a project with detailed lifecycle information.
    /// </summary>
    public class Project
    {
        public Guid Id { get; set; } // Unique project identifier

        public string Name { get; set; } = string.Empty; // Project name
        public string Description { get; set; } = string.Empty; // Detailed description

        public Guid DepartmentId { get; set; } // FK to department
        public Department? Department { get; set; } // Navigation property

        public Guid ProjectManagerId { get; set; } // FK to project manager
        public User? ProjectManager { get; set; } // Navigation property

        public Guid? StrategicGoalId { get; set; } // Optional FK to strategic goal
        public StrategicGoal? StrategicGoal { get; set; }

        public Guid? AnnualGoalId { get; set; } // Optional FK to annual goal
        public AnnualGoal? AnnualGoal { get; set; }

        public DateTime StartDate { get; set; } // Start date
        public DateTime EndDate { get; set; }   // Deadline

        public decimal Budget { get; set; } // Approved budget
        public decimal ActualCost { get; set; } // Incurred cost
        public decimal EstimatedCost { get; set; } // New: Estimated cost
        public string ClientName { get; set; } = string.Empty; // New: Client name

        public StatusColor StatusColor { get; set; } // Visual status based on deadline
        public ProjectStatus Status { get; set; } // Overall project status

        public DateTime CreationDate { get; set; } // Creation timestamp
        public DateTime? ApprovedDate { get; set; } // Approval timestamp (if applicable)

        public string Category { get; set; } = string.Empty; // Category

        // Navigation collections for related entities.
        public ICollection<WeeklyUpdate>? WeeklyUpdates { get; set; }
        public ICollection<ChangeRequest>? ChangeRequests { get; set; }
        public ICollection<Document>? Documents { get; set; }
        public ICollection<ProjectMember>? Members { get; set; }

        /// <summary>
        /// Computes the status color based on remaining days until deadline.
        /// </summary>
        public StatusColor ComputeStatusColor()
        {
            var daysLeft = (EndDate - DateTime.UtcNow).TotalDays;
            if (daysLeft > 30)
                return StatusColor.Green;
            else if (daysLeft > 10)
                return StatusColor.Yellow;
            else
                return StatusColor.Red;
        }
    }
}
