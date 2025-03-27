using System;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Enumerates the possible statuses for a milestone.
    /// </summary>
    public enum MilestoneStatus
    {
        Pending,
        InProgress,
        Completed,
        Missed
    }

    /// <summary>
    /// Represents a milestone within a project timeline.
    /// </summary>
    public class ProjectMilestone
    {
        public Guid Id { get; set; } // Unique identifier

        public Guid ProjectId { get; set; } // FK to project
        public Project? Project { get; set; } // Navigation property

        public string Name { get; set; } = string.Empty; // Milestone name
        
        public string Description { get; set; } = string.Empty; // Detailed description

        public DateTime Date { get; set; } // Milestone date
        
        public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending; // Current status
        
        public Guid? ResponsibleUserId { get; set; } // FK to user responsible (optional)
        public User? ResponsibleUser { get; set; } // Navigation property
        
        public DateTime CreatedDate { get; set; } // Creation timestamp
        
        public DateTime? CompletedDate { get; set; } // Completion timestamp (if applicable)
        
        // Helper method to check if the milestone is overdue
        public bool IsOverdue()
        {
            return Status != MilestoneStatus.Completed && Date < DateTime.UtcNow;
        }
        
        // Helper method to get the days remaining until milestone
        public int GetDaysRemaining()
        {
            if (Status == MilestoneStatus.Completed || Date < DateTime.UtcNow)
            {
                return 0;
            }
            
            return (int)Math.Ceiling((Date - DateTime.UtcNow).TotalDays);
        }
    }
}