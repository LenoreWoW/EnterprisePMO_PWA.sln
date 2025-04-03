using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EnterprisePMO_PWA.Domain.Enums;
using Supabase.Postgrest.Models;
using TableAttribute = Supabase.Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Supabase.Postgrest.Attributes.ColumnAttribute;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a project with detailed lifecycle information.
    /// </summary>
    [Table("projects")]
    public class Project : BaseModel
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("department_id")]
        public Guid DepartmentId { get; set; }

        [Column("project_manager_id")]
        public Guid ProjectManagerId { get; set; }

        [Column("strategic_goal_id")]
        public Guid? StrategicGoalId { get; set; }

        [Column("annual_goal_id")]
        public Guid? AnnualGoalId { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("budget")]
        public decimal Budget { get; set; }

        [Column("actual_cost")]
        public decimal ActualCost { get; set; }

        [Column("estimated_cost")]
        public decimal EstimatedCost { get; set; }

        [Column("client_name")]
        public string ClientName { get; set; }

        [Column("status_color")]
        public StatusColor StatusColor { get; set; }

        [Column("status")]
        public ProjectStatus Status { get; set; }

        [Column("percent_complete")]
        public int PercentComplete { get; set; }

        [Column("creation_date")]
        public DateTime CreationDate { get; set; }

        [Column("approved_date")]
        public DateTime? ApprovedDate { get; set; }

        [Column("category")]
        public string Category { get; set; }

        [Column("progress")]
        public int Progress { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("ProjectManagerId")]
        public virtual User ProjectManager { get; set; }

        [ForeignKey("StrategicGoalId")]
        public virtual StrategicGoal StrategicGoal { get; set; }

        [ForeignKey("AnnualGoalId")]
        public virtual AnnualGoal AnnualGoal { get; set; }

        public virtual ICollection<ProjectMember> Members { get; set; }
        public virtual ICollection<ProjectTask> Tasks { get; set; }
        public virtual ICollection<WeeklyUpdate> WeeklyUpdates { get; set; }
        public virtual ICollection<ChangeRequest> ChangeRequests { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<ProjectUpdate> Updates { get; set; }

        /// <summary>
        /// Computes the status color based on project progress and timeline.
        /// </summary>
        public StatusColor ComputeStatusColor()
        {
            if (Status == ProjectStatus.Rejected)
                return StatusColor.Red;

            if (DateTime.UtcNow > EndDate)
                return StatusColor.Red;

            if (PercentComplete < 80)
                return StatusColor.Yellow;

            if (Status == ProjectStatus.Active)
                return StatusColor.Green;
            return StatusColor.Blue;
        }
    }

    public static class ProjectExtensions
    {
        public static string GetStatusClass(this Project project) => project.Status switch
        {
            ProjectStatus.Draft => "bg-gray-100 text-gray-800",
            ProjectStatus.Active => "bg-blue-100 text-blue-800",
            ProjectStatus.Rejected => "bg-red-100 text-red-800",
            ProjectStatus.Completed => "bg-green-100 text-green-800",
            _ => "bg-gray-100 text-gray-800"
        };

        public static string GetProgressBarClass(this Project project) => project.PercentComplete switch
        {
            < 25 => "bg-red-600",
            < 50 => "bg-yellow-600",
            < 75 => "bg-blue-600",
            _ => "bg-green-600"
        };
    }
}