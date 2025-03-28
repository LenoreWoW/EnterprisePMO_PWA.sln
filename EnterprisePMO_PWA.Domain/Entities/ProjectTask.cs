using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterprisePMO_PWA.Domain.Entities
{
    public class ProjectTask
    {
        [Key]
        public Guid Id { get; set; }
        
        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        
        public DateTime DueDate { get; set; }
        
        public ProjectTaskStatus Status { get; set; }
        
        public int Priority { get; set; }
        
        [ForeignKey("Project")]
        public Guid ProjectId { get; set; }
        public virtual Project? Project { get; set; }
        
        [ForeignKey("AssignedTo")]
        public Guid? AssignedToId { get; set; }
        public virtual User? AssignedTo { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedDate { get; set; }
        
        public int PercentComplete { get; set; }
    }
    
    public enum ProjectTaskStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Delayed,
        Cancelled
    }
}