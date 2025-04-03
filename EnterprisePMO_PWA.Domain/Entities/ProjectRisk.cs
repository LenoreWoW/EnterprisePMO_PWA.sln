using System;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a risk associated with a project.
    /// </summary>
    [Table("project_risks")]
    public class ProjectRisk : BaseModel
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("project_id")]
        public int ProjectId { get; set; }

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("impact")]
        public RiskImpact Impact { get; set; }

        [Column("mitigation_strategy")]
        public string MitigationStrategy { get; set; } = string.Empty;

        [Column("status")]
        public RiskStatus Status { get; set; }

        // Navigation property
        public Project? Project { get; set; }
    }

    /// <summary>
    /// Enumerates the possible impact levels of a risk.
    /// </summary>
    public enum RiskImpact
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Enumerates the possible statuses of a risk.
    /// </summary>
    public enum RiskStatus
    {
        Identified,
        Assessed,
        Mitigated,
        Closed
    }
} 