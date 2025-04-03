using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterprisePMO_PWA.Domain.Entities
{
    /// <summary>
    /// Represents a user's assignment to a project with a specific role.
    /// </summary>
    public class ProjectMember
    {
        public Guid Id { get; set; } // Unique membership identifier

        public Guid ProjectId { get; set; } // FK to project
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } // Navigation property

        public Guid UserId { get; set; } // FK to user
        [ForeignKey("UserId")]
        public virtual User User { get; set; } // Navigation property

        public Guid RoleId { get; set; } // FK to assigned role
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } // Navigation property
    }

    public static class ProjectMemberExtensions
    {
        private static readonly string[] AvatarColors = new[]
        {
            "bg-blue-600",
            "bg-green-600",
            "bg-yellow-600",
            "bg-red-600",
            "bg-purple-600",
            "bg-pink-600",
            "bg-indigo-600"
        };

        public static string GetAvatarClass(this ProjectMember member)
        {
            var colorIndex = Math.Abs(member.UserId.GetHashCode()) % AvatarColors.Length;
            return AvatarColors[colorIndex];
        }

        public static string GetInitials(this ProjectMember member)
        {
            if (member.User?.Username == null) return "?";
            var parts = member.User.Username.Split(' ');
            if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
            return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
        }

        public static string GetName(this ProjectMember member)
        {
            return member.User?.Username ?? "Unknown User";
        }
    }
}
