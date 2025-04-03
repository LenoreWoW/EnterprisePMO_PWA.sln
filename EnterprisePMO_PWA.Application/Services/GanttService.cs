using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using EnterprisePMO_PWA.Domain.Entities;
using Microsoft.Extensions.Logging;
using Supabase.Postgrest;
using static Supabase.Postgrest.Constants;

namespace EnterprisePMO_PWA.Application.Services
{
    public class GanttService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly ILogger<GanttService> _logger;

        public GanttService(Supabase.Client supabaseClient, ILogger<GanttService> logger)
        {
            _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<GanttTask>> GetProjectTasksAsync(int projectId)
        {
            try
            {
                var response = await _supabaseClient
                    .From<ProjectTask>()
                    .Filter("project_id", Operator.Equals, projectId)
                    .Order("start_date", Ordering.Ascending)
                    .Get();

                return response.Models.Select(t => new GanttTask
                {
                    Id = t.Id,
                    Text = t.Title,
                    StartDate = t.StartDate,
                    EndDate = t.DueDate ?? t.StartDate.AddDays(1),
                    Progress = t.Progress,
                    Parent = t.ParentTaskId
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Gantt tasks for project {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<List<GanttDependency>> GetTaskDependenciesAsync(int projectId)
        {
            try
            {
                var response = await _supabaseClient
                    .From<ProjectDependency>()
                    .Filter("project_id", Operator.Equals, projectId)
                    .Get();

                return response.Models.Select(d => new GanttDependency
                {
                    Id = d.Id,
                    Source = d.SourceTaskId,
                    Target = d.TargetTaskId,
                    Type = d.DependencyType.ToString().ToLower()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task dependencies for project {ProjectId}", projectId);
                throw;
            }
        }

        public async Task UpdateTaskProgressAsync(int taskId, int progress)
        {
            try
            {
                var task = new ProjectTask { Id = taskId, Progress = progress };
                await _supabaseClient
                    .From<ProjectTask>()
                    .Filter("id", Operator.Equals, taskId)
                    .Update(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task progress for task {TaskId}", taskId);
                throw;
            }
        }

        public async Task UpdateTaskDatesAsync(int taskId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var task = new ProjectTask { Id = taskId, StartDate = startDate, DueDate = endDate };
                await _supabaseClient
                    .From<ProjectTask>()
                    .Filter("id", Operator.Equals, taskId)
                    .Update(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task dates for task {TaskId}", taskId);
                throw;
            }
        }
    }

    public class GanttTask
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Progress { get; set; }
        public int? Parent { get; set; }
    }

    public class GanttDependency
    {
        public int Id { get; set; }
        public int Source { get; set; }
        public int Target { get; set; }
        public string Type { get; set; } = string.Empty;
    }
} 