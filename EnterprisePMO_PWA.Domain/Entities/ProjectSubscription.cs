using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TableAttribute = Supabase.Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Supabase.Postgrest.Attributes.ColumnAttribute;

namespace EnterprisePMO_PWA.Domain.Entities
{
    [Table("project_subscriptions")]
    public class ProjectSubscription
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("project_id")]
        public Guid ProjectId { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Column("updated_date")]
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }
    }
} 