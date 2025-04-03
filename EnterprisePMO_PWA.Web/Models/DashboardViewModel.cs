using System;
using System.Collections.Generic;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;

namespace EnterprisePMO_PWA.Web.Models
{
    public class DashboardViewModel
    {
        // Statistics
        public int ActiveProjects { get; set; }
        public int PendingTasks { get; set; }
        public int PendingChangeRequests { get; set; }
        public int OverdueTasks { get; set; }
        public int OverallProgress { get; set; }
        
        // Recent Projects
        public List<ProjectCardViewModel> RecentProjects { get; set; } = new List<ProjectCardViewModel>();
        
        // Upcoming Deadlines
        public List<DeadlineViewModel> UpcomingDeadlines { get; set; } = new List<DeadlineViewModel>();
        
        // Stats changes (for comparison with previous period)
        public StatsChangeViewModel StatsChanges { get; set; } = new StatsChangeViewModel();

        public string ProjectName { get; set; } = string.Empty;
        public List<Project> CompletedProjects { get; set; } = new();
        public List<ProjectMember> TeamMembers { get; set; } = new();
        public List<ActivityLog> RecentActivities { get; set; } = new();
        public List<Project> ActiveProjectList { get; set; } = new();
        public StatsChangeViewModel StatsChange { get; set; } = new();
    }
    
    public class DeadlineViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public DateTime DueDate { get; set; }
        public StatusColor Status { get; set; }
        public int PercentComplete { get; set; }
        public int DaysLeft { get; set; }
        
        public string StatusText => Status switch
        {
            StatusColor.Green => "On Track",
            StatusColor.Yellow => "At Risk",
            StatusColor.Red => "Delayed",
            StatusColor.Blue => "Not Started",
            StatusColor.Purple => "On Hold",
            StatusColor.Gray => "Completed",
            _ => "Unknown"
        };

        public string StatusClass => Status switch
        {
            StatusColor.Green => "bg-green-100 text-green-800",
            StatusColor.Yellow => "bg-yellow-100 text-yellow-800",
            StatusColor.Red => "bg-red-100 text-red-800",
            StatusColor.Blue => "bg-blue-100 text-blue-800",
            StatusColor.Purple => "bg-purple-100 text-purple-800",
            StatusColor.Gray => "bg-gray-100 text-gray-800",
            _ => "bg-gray-100 text-gray-800"
        };

        public string ProgressBarClass => Status switch
        {
            StatusColor.Green => "bg-green-500",
            StatusColor.Yellow => "bg-yellow-500",
            StatusColor.Red => "bg-red-500",
            StatusColor.Blue => "bg-blue-500",
            StatusColor.Purple => "bg-purple-500",
            StatusColor.Gray => "bg-gray-500",
            _ => "bg-primary-600"
        };
        
        public string DateFormatted => DueDate.ToString("MMM dd, yyyy");
        
        public string DaysLeftText => DaysLeft switch
        {
            0 => "Due today",
            1 => "Due tomorrow",
            _ => $"{DaysLeft} days left"
        };
        
        public bool IsUrgent => DaysLeft <= 5;
    }
    
    public class StatsChangeViewModel
    {
        public int ActiveProjectsChange { get; set; }
        public int PendingTasksChange { get; set; }
        public int PendingChangeRequestsChange { get; set; }
        public int OverdueTasksChange { get; set; }
        
        public bool IsActiveProjectsPositive => ActiveProjectsChange >= 0;
        public bool IsPendingTasksPositive => PendingTasksChange <= 0; // Fewer pending tasks is positive
        public bool IsPendingChangeRequestsPositive => PendingChangeRequestsChange <= 0; // Fewer pending requests is positive
        public bool IsOverdueTasksPositive => OverdueTasksChange <= 0; // Fewer overdue tasks is positive

        public bool IsCompletedProjectsPositive { get; set; }
        public decimal CompletedProjectsChange { get; set; }
        public bool IsTeamMembersPositive { get; set; }
        public decimal TeamMembersChange { get; set; }
    }

    public class ActivityLog
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
    }
}