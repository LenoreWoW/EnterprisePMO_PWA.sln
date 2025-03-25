using System;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents an attachment (document or image) associated with a project.
    /// </summary>
    public class Document
    {
        public Guid Id { get; set; } // Unique identifier

        public Guid ProjectId { get; set; } // FK to project
        public Project? Project { get; set; } // Navigation property

        public string FileName { get; set; } = string.Empty; // File name
        public string FilePath { get; set; } = string.Empty;   // Storage path or URL

        public DateTime UploadDate { get; set; } // Timestamp of upload

        public Guid UploadedByUserId { get; set; } // Uploader's ID
        public User? UploadedByUser { get; set; } // Navigation property
    }
}
