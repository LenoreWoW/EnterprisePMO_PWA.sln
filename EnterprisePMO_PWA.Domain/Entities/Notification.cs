using System;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Defines notification types within the system.
    /// </summary>
    public enum NotificationType
    {
        ProjectCreation,
        ProjectSubmission,
        ProjectApproval,
        ProjectRejection,
        WeeklyUpdateReminder,
        WeeklyUpdateSubmission,
        WeeklyUpdateApproval,
        ChangeRequestSubmission,
        ChangeRequestApproval,
        ChangeRequestRejection,
        SystemAlert,
        MentionNotification,
        TaskAssignment,
        DeadlineReminder
    }

    /// <summary>
    /// Represents a notification to a user within the system.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Unique identifier for the notification.
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The recipient user's ID.
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Navigation property to the user.
        /// </summary>
        public User? User { get; set; }
        
        /// <summary>
        /// The notification message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// The type of notification.
        /// </summary>
        public NotificationType Type { get; set; }
        
        /// <summary>
        /// Link to the relevant entity in the system.
        /// </summary>
        public string? Link { get; set; }
        
        /// <summary>
        /// Optional reference to a related entity ID.
        /// </summary>
        public Guid? EntityId { get; set; }
        
        /// <summary>
        /// Optional entity type (e.g., "Project", "WeeklyUpdate").
        /// </summary>
        public string? EntityType { get; set; }
        
        /// <summary>
        /// Creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Whether the notification has been read.
        /// </summary>
        public bool IsRead { get; set; } = false;
        
        /// <summary>
        /// When the notification was read (if applicable).
        /// </summary>
        public DateTime? ReadAt { get; set; }
        
        /// <summary>
        /// Whether an email was sent for this notification.
        /// </summary>
        public bool EmailSent { get; set; } = false;
    }
}