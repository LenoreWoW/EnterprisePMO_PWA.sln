using System;
using System.Collections.Generic;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;

namespace EnterprisePMO_PWA.Web.Models
{
    public class ProjectDetailsViewModel
    {
        public Project Project { get; set; } = null!;
        public List<ProjectTask> Tasks { get; set; } = new();
        public List<ProjectRisk> Risks { get; set; } = new();
        public List<ChangeRequest> ChangeRequests { get; set; } = new();
        public List<Document> Documents { get; set; } = new();
        public List<WeeklyUpdate> WeeklyUpdates { get; set; } = new();
        public List<ProjectMember> Members { get; set; } = new();

        // Properties used in views
        public Guid Id => Project.Id;
        public string Name => Project.Name;
        public string Description => Project.Description;
        public DateTime StartDate => Project.StartDate;
        public DateTime EndDate => Project.EndDate;
        public string StatusText => Project.Status.ToString();
        public string StatusClass => GetStatusClass();
        public string StatusColor => GetStatusColor();
        public int DaysRemaining => (Project.EndDate - DateTime.Now).Days;
        public string ClientName => Project.ClientName;
        public string Category => Project.Category;
        public Department? Department => Project.Department;
        public User? ProjectManager => Project.ProjectManager;
        public StrategicGoal? StrategicGoal => Project.StrategicGoal;
        public AnnualGoal? AnnualGoal => Project.AnnualGoal;
        public decimal Budget => Project.Budget;
        public decimal ActualCost => Project.ActualCost;
        public decimal EstimatedCost => Project.EstimatedCost;
        public decimal PercentComplete => Project.PercentComplete;
        public StatusColor StatusColorEnum => Project.StatusColor;
        public decimal PlannedValue => Project.PlannedValue;
        public decimal EarnedValue => Project.EarnedValue;
        public decimal CostPerformanceIndex => Project.CostPerformanceIndex;
        public decimal SchedulePerformanceIndex => Project.SchedulePerformanceIndex;
        public decimal EstimateAtCompletion => Project.EstimateAtCompletion;
        public decimal VarianceAtCompletion => Project.VarianceAtCompletion;
        public decimal CompletionPercentage => Project.CompletionPercentage;
        public ProjectStatus Status => Project.Status;
        public string ProgressBarClass => GetProgressBarClass();
        public List<ProjectRisk> RiskAssessment => Risks;
        public List<ProjectMember> TeamMembers => Members;

        private string GetStatusClass()
        {
            return Status switch
            {
                ProjectStatus.Draft => "bg-secondary",
                ProjectStatus.PendingApproval => "bg-warning",
                ProjectStatus.Active => "bg-primary",
                ProjectStatus.OnHold => "bg-info",
                ProjectStatus.Completed => "bg-success",
                ProjectStatus.Cancelled => "bg-danger",
                ProjectStatus.Rejected => "bg-danger",
                _ => "bg-secondary"
            };
        }

        private string GetStatusColor()
        {
            return Status switch
            {
                ProjectStatus.Draft => "#6c757d",
                ProjectStatus.PendingApproval => "#ffc107",
                ProjectStatus.Active => "#0d6efd",
                ProjectStatus.OnHold => "#0dcaf0",
                ProjectStatus.Completed => "#198754",
                ProjectStatus.Cancelled => "#dc3545",
                ProjectStatus.Rejected => "#dc3545",
                _ => "#6c757d"
            };
        }

        private string GetProgressBarClass()
        {
            if (CompletionPercentage >= 100)
                return "bg-success";
            if (CompletionPercentage >= 75)
                return "bg-info";
            if (CompletionPercentage >= 50)
                return "bg-primary";
            if (CompletionPercentage >= 25)
                return "bg-warning";
            return "bg-danger";
        }
    }
} 