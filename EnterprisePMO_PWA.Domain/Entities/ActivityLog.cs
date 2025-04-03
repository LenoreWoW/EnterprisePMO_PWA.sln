using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EnterprisePMO_PWA.Domain.Enums;
using TableAttribute = Supabase.Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Supabase.Postgrest.Attributes.ColumnAttribute;

namespace EnterprisePMO_PWA.Domain.Entities
{
    [Table("activity_logs")]
    public class ActivityLog
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("project_id")]
        public Guid? ProjectId { get; set; }

        [Column("entity_type")]
        public string EntityType { get; set; }

        [Column("entity_id")]
        public string EntityId { get; set; }

        [Column("action")]
        public string Action { get; set; }

        [Column("details")]
        public string Details { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }
    }
} 