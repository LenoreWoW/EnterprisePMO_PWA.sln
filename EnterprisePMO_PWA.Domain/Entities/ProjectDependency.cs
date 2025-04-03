using System;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a dependency between two project tasks.
    /// </summary>
    [Table("project_dependencies")]
    public class ProjectDependency : BaseModel
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("project_id")]
        public int ProjectId { get; set; }

        [Column("source_task_id")]
        public int SourceTaskId { get; set; }

        [Column("target_task_id")]
        public int TargetTaskId { get; set; }

        [Column("dependency_type")]
        public DependencyType DependencyType { get; set; }

        // Navigation properties
        public ProjectTask? SourceTask { get; set; }
        public ProjectTask? TargetTask { get; set; }
    }

    /// <summary>
    /// Enumerates the possible types of task dependencies.
    /// </summary>
    public enum DependencyType
    {
        FinishToStart,
        StartToStart,
        FinishToFinish,
        StartToFinish
    }
} 