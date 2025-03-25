using System;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Enumerates the possible statuses for a change request.
    /// </summary>
    public enum ChangeRequestStatus
    {
        Proposed,
        SubPMOApproved,
        MainPMOApproved,
        Rejected
    }

    /// <summary>
    /// Represents a request to change project scope, budget, or timeline.
    /// </summary>
    public class ChangeRequest
    {
        public Guid Id { get; set; } // Unique identifier

        public Guid ProjectId { get; set; } // FK to the project
        public Project? Project { get; set; } // Navigation property

        public Guid RequestedByUserId { get; set; } // User who requested the change
        public User? RequestedByUser { get; set; } // Navigation property

        public DateTime RequestDate { get; set; } // Timestamp when the request was made

        public string ChangeType { get; set; } = string.Empty; // e.g., Scope, Budget, Timeline
        public string Justification { get; set; } = string.Empty; // Reason for the change
        public string ImpactAnalysis { get; set; } = string.Empty; // Analysis of impact

        public DateTime? SubPMOApprovalDate { get; set; } // Approval date by Sub PMO
        public DateTime? MainPMOApprovalDate { get; set; } // Approval date by Main PMO

        public ChangeRequestStatus ApprovalStatus { get; set; } // Current status
    }
}
