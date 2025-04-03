using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TableAttribute = Supabase.Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Supabase.Postgrest.Attributes.ColumnAttribute;
using EnterprisePMO_PWA.Domain.Enums;
using Supabase.Postgrest.Models;

namespace EnterprisePMO_PWA.Domain.Entities
{
    [Table("user_notification_preferences")]
    public class UserNotificationPreferences : BaseModel
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("email_enabled")]
        public bool EmailEnabled { get; set; }

        [Column("push_enabled")]
        public bool PushEnabled { get; set; }

        [Column("project_updates")]
        public bool ProjectUpdates { get; set; }

        [Column("task_updates")]
        public bool TaskUpdates { get; set; }

        [Column("comment_updates")]
        public bool CommentUpdates { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Column("push_notifications_enabled")]
        public bool PushNotificationsEnabled { get; set; }

        [Column("email_digest_enabled")]
        public bool EmailDigestEnabled { get; set; }

        [Column("email_digest_frequency")]
        public EmailDigestFrequency EmailDigestFrequency { get; set; }

        [Column("critical_notifications_enabled")]
        public bool CriticalNotificationsEnabled { get; set; }

        [Column("system_announcements_enabled")]
        public bool SystemAnnouncementsEnabled { get; set; }

        [Column("last_digest_sent")]
        public DateTime? LastDigestSent { get; set; }

        public UserNotificationPreferences()
        {
            PushEnabled = true;
            EmailEnabled = true;
            ProjectUpdates = true;
            TaskUpdates = true;
            CommentUpdates = true;
            PushNotificationsEnabled = true;
            EmailDigestEnabled = false;
            EmailDigestFrequency = EmailDigestFrequency.Daily;
            CriticalNotificationsEnabled = true;
            SystemAnnouncementsEnabled = true;
            CreatedDate = DateTime.UtcNow;
        }
    }
} 