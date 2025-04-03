using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EnterprisePMO_PWA.Domain.Enums;
using TableAttribute = Supabase.Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Supabase.Postgrest.Attributes.ColumnAttribute;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Enumerates the possible types for a change request.
    /// </summary>
    public enum ChangeRequestType
    {
        Scope,
        Schedule,
        Budget,
        Quality
    }

    /// <summary>
    /// Enumerates the possible impact levels for a change request.
    /// </summary>
    public enum ChangeRequestImpact
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Enumerates the possible statuses for a change request.
    /// </summary>
    public enum ChangeRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Implemented
    }

    /// <summary>
    /// Represents a request to change project scope, budget, or timeline.
    /// </summary>
    [Table("change_requests")]
    public class ChangeRequest
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("project_id")]
        public Guid ProjectId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("requested_by_id")]
        public Guid RequestedById { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("request_date")]
        public DateTime RequestDate { get; set; }

        [Column("main_pmo_approval_date")]
        public DateTime? MainPMOApprovalDate { get; set; }

        [Column("sub_pmo_approval_date")]
        public DateTime? SubPMOApprovalDate { get; set; }

        [Column("status")]
        public ChangeRequestStatus Status { get; set; }

        [Column("approved_by_id")]
        public Guid? ApprovedById { get; set; }

        [Column("approved_date")]
        public DateTime? ApprovedDate { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [ForeignKey("RequestedById")]
        public virtual User RequestedBy { get; set; }

        [ForeignKey("ApprovedById")]
        public virtual User ApprovedBy { get; set; }

        public virtual User RequestedByUser => RequestedBy;
    }
}
