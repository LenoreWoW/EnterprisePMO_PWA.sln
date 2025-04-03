using System;
using EnterprisePMO_PWA.Domain.Enums;
using Supabase.Postgrest.Attributes;

namespace EnterprisePMO_PWA.Domain.Entities
{
    [Table("email_digest_items")]
    public class EmailDigestItem : BaseEntityModel
    {
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("notification_id")]
        public string NotificationId { get; set; } = string.Empty;

        [Column("processed")]
        public bool Processed { get; set; }

        // Navigation property - not mapped to database
        [Reference(typeof(Notification))]
        public Notification? Notification { get; set; }

        public EmailDigestItem()
        {
            Processed = false;
        }
    }
} 