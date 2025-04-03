using System;
using Supabase.Postgrest.Attributes;

namespace EnterprisePMO_PWA.Domain.Entities
{
    [Table("push_subscriptions")]
    public class PushSubscription : BaseEntityModel
    {
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("endpoint")]
        public string Endpoint { get; set; } = string.Empty;

        [Column("p256dh")]
        public string P256dh { get; set; } = string.Empty;

        [Column("auth")]
        public string Auth { get; set; } = string.Empty;

        [Column("last_used")]
        public DateTime? LastUsed { get; set; }

        public PushSubscription()
        {
            LastUsed = DateTime.UtcNow;
        }
    }
} 