using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TableAttribute = Supabase.Postgrest.Attributes.TableAttribute;
using ColumnAttribute = Supabase.Postgrest.Attributes.ColumnAttribute;

namespace EnterprisePMO_PWA.Domain.Entities
{
    [Table("project_updates")]
    public class ProjectUpdate
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("project_id")]
        public Guid ProjectId { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("notes")]
        public string Notes { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
} 