using System;
using System.ComponentModel.DataAnnotations;
using EnterprisePMO_PWA.Domain.Enums;
using Supabase.Postgrest.Attributes;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a notification to a user within the system.
    /// </summary>
    [Table("notifications")]
    public class Notification : BaseEntityModel
    {
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("title")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("notification_type")]
        public NotificationType Type { get; set; }

        [Column("priority")]
        public NotificationPriority Priority { get; set; }

        [Column("related_entity_id")]
        public string? RelatedEntityId { get; set; }

        [Column("related_entity_type")]
        public string? RelatedEntityType { get; set; }

        [Column("read")]
        public bool Read { get; set; }

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }

        [Column("action_url")]
        public string? ActionUrl { get; set; }

        public string? Icon { get; set; }

        public string? Category { get; set; }

        public string? GroupId { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public bool IsArchived { get; set; }

        public DateTime? ArchivedAt { get; set; }

        public string? ArchivedBy { get; set; }

        public string? Metadata { get; set; }

        public Notification()
        {
            Type = NotificationType.SystemAlert;
            Priority = NotificationPriority.Medium;
            Read = false;
        }
    }
}