using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterprisePMO_PWA.Domain.Entities
{
    public class Project
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public ProjectStatus Status { get; set; }
        
        public decimal Budget { get; set; }
        
        public decimal EstimatedCost { get; set; }
        
        public string ClientName { get; set; } = string.Empty;
        
        public decimal PercentComplete { get; set; }
        
        public string StatusColor { get; set; } = string.Empty;
        
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? ApprovedDate { get; set; }
        
        [ForeignKey("Department")]
        public Guid DepartmentId { get; set; }
        public virtual Department? Department { get; set; }
        
        [ForeignKey("ProjectManager")]
        public Guid ProjectManagerId { get; set; }
        public virtual User? ProjectManager { get; set; }
        
        [ForeignKey("StrategicGoal")]
        public Guid? StrategicGoalId { get; set; }
        public virtual StrategicGoal? StrategicGoal { get; set; }
        
        [ForeignKey("AnnualGoal")]
        public Guid? AnnualGoalId { get; set; }
        public virtual AnnualGoal? AnnualGoal { get; set; }
        
        public string Category { get; set; } = string.Empty;
        
        public virtual ICollection<WeeklyUpdate> WeeklyUpdates { get; set; } = new List<WeeklyUpdate>();
        public virtual ICollection<ChangeRequest> ChangeRequests { get; set; } = new List<ChangeRequest>();
        public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

        /// <summary>
        /// Computes the status color based on project metrics
        /// </summary>
        public string ComputeStatusColor()
        {
            // Default implementation
            if (Status == ProjectStatus.Completed)
                return "Green";
            else if (Status == ProjectStatus.Rejected)
                return "Red";
                
            // Calculate based on schedule and budget
            var today = DateTime.UtcNow;
            var totalDuration = (EndDate - StartDate).TotalDays;
            var elapsedDuration = (today - StartDate).TotalDays;
            
            // Calculate expected progress percentage based on time elapsed
            var expectedProgress = Math.Min(100, Math.Max(0, (elapsedDuration / totalDuration) * 100));
            
            // Determine status color
            if (PercentComplete >= expectedProgress - 10)
                return "Green";
            else if (PercentComplete >= expectedProgress - 20)
                return "Yellow";
            else
                return "Red";
        }
    }
    
    public enum ProjectStatus
    {
        Proposed,
        Active,
        OnHold,
        Completed,
        Cancelled,
        Rejected
    }
}