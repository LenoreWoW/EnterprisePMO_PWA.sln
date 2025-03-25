using System;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a weekly update submitted by a project manager.
    /// </summary>
    public class WeeklyUpdate
    {
        public Guid Id { get; set; } // Unique identifier

        public Guid ProjectId { get; set; } // FK to project
        public Project? Project { get; set; } // Navigation property

        public DateTime WeekEndingDate { get; set; } // Week ending date

        public string Accomplishments { get; set; } = string.Empty; // Weekly accomplishments

        public string NextSteps { get; set; } = string.Empty; // Planned next steps

        public string IssuesOrRisks { get; set; } = string.Empty; // Issues or risks encountered

        public int PercentComplete { get; set; } // Completion percentage

        public StatusColor StatusColor { get; set; } // Visual status indicator

        public bool IsApprovedBySubPMO { get; set; } // Approval flag

        public bool IsSentBack { get; set; } // Flag if update was sent back for revision

        public Guid SubmittedByUserId { get; set; } // FK to user who submitted update
        public User? SubmittedByUser { get; set; } // Navigation property

        public DateTime SubmittedDate { get; set; } // Submission timestamp

        public DateTime? ApprovedDate { get; set; } // Approval timestamp (if applicable)
    }
}
