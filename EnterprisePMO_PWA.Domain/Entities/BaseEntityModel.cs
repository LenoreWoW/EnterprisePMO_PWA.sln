using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace EnterprisePMO_PWA.Domain.Entities
{
    public class BaseEntityModel : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public BaseEntityModel()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
} 