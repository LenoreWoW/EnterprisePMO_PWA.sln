using System;
using EnterprisePMO_PWA.Domain.Enums;
using Supabase.Postgrest.Attributes;

namespace EnterprisePMO_PWA.Domain.Entities
{
    [Table("role_notification_preferences")]
    public class RoleNotificationPreferences : BaseEntityModel
    {
        [Column("role_name")]
        public string RoleName { get; set; } = string.Empty;

        [Column("push_notifications_enabled")]
        public bool PushNotificationsEnabled { get; set; }

        [Column("email_digest_enabled")]
        public bool EmailDigestEnabled { get; set; }

        [Column("email_digest_frequency")]
        public EmailDigestFrequency EmailDigestFrequency { get; set; }

        [Column("critical_notifications_enabled")]
        public bool CriticalNotificationsEnabled { get; set; }

        [Column("project_updates_enabled")]
        public bool ProjectUpdatesEnabled { get; set; }

        [Column("task_updates_enabled")]
        public bool TaskUpdatesEnabled { get; set; }

        [Column("system_announcements_enabled")]
        public bool SystemAnnouncementsEnabled { get; set; }

        public RoleNotificationPreferences()
        {
            PushNotificationsEnabled = true;
            EmailDigestEnabled = false;
            EmailDigestFrequency = EmailDigestFrequency.Daily;
            CriticalNotificationsEnabled = true;
            ProjectUpdatesEnabled = true;
            TaskUpdatesEnabled = true;
            SystemAnnouncementsEnabled = true;
        }
    }
} 