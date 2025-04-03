using System;
using System.Collections.Generic;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using TaskStatusEnum = EnterprisePMO_PWA.Domain.Enums.TaskStatus;

namespace EnterprisePMO_PWA.Domain.Entities
{
    public static class ProjectTaskExtensions
    {
        private static readonly string[] AvatarColors = { "blue", "green", "yellow", "red", "purple", "pink" };

        public static string GetAssigneeAvatarClass(this ProjectTask task)
        {
            var colorIndex = Math.Abs(task.AssignedToId ?? 0) % AvatarColors.Length;
            return $"bg-{AvatarColors[colorIndex]}-100";
        }

        public static string GetAssigneeInitials(this ProjectTask task)
        {
            var username = task.AssignedTo?.Username ?? "";
            if (string.IsNullOrEmpty(username)) return "?";
            var parts = username.Split('@')[0].Split('.');
            if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
            return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
        }

        public static string GetAssigneeName(this ProjectTask task)
        {
            return task.AssignedTo?.Username ?? "Unassigned";
        }

        public static string GetStatusClass(this ProjectTask task)
        {
            return task.Status switch
            {
                TaskStatusEnum.ToDo => "bg-gray-100 text-gray-800",
                TaskStatusEnum.InProgress => "bg-blue-100 text-blue-800",
                TaskStatusEnum.Review => "bg-yellow-100 text-yellow-800",
                TaskStatusEnum.Completed => "bg-green-100 text-green-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }

        public static string GetStatusText(this ProjectTask task)
        {
            return task.Status.ToString();
        }

        public static string GetPriorityClass(this ProjectTask task)
        {
            return task.Priority.ToLower() switch
            {
                "high" => "bg-red-100 text-red-800",
                "medium" => "bg-yellow-100 text-yellow-800",
                "low" => "bg-green-100 text-green-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }
    }

    /// <summary>
    /// Represents a task within a project.
    /// </summary>
    [Table("project_tasks")]
    public class ProjectTask : BaseModel
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("project_id")]
        public int ProjectId { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime? DueDate { get; set; }

        [Column("status")]
        public TaskStatusEnum Status { get; set; }

        [Column("priority")]
        public string Priority { get; set; } = "Medium";

        [Column("assigned_to_id")]
        public int? AssignedToId { get; set; }

        [Column("progress")]
        public int Progress { get; set; }

        [Column("parent_task_id")]
        public int? ParentTaskId { get; set; }

        // Navigation properties
        public Project? Project { get; set; }
        public User? AssignedTo { get; set; }
        public ProjectTask? ParentTask { get; set; }
        public ICollection<ProjectTask>? Subtasks { get; set; }
        public ICollection<ProjectDependency>? Dependencies { get; set; }

        public bool IsCompleted => Status == TaskStatusEnum.Completed;
    }
}