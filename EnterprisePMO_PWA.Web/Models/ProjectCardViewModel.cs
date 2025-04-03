using System;
using System.Collections.Generic;
using System.Linq;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;

namespace EnterprisePMO_PWA.Web.Models
{
    public class ProjectCardViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Progress => Project?.PercentComplete ?? 0;
        public bool HasProgress => Project?.PercentComplete > 0;
        public decimal ProgressValue => Project?.PercentComplete ?? 0;
        public IEnumerable<ProjectMember> TeamMembers => Project?.Members ?? new List<ProjectMember>();
        public bool IsTaskCompleted(ProjectTask task) => task.Status == Domain.Enums.TaskStatus.Done;
        public DateTime EndDate => Project?.EndDate ?? DateTime.MinValue;
        public ProjectStatus Status => Project?.Status ?? ProjectStatus.Draft;
        public string StatusText => Status.ToString();
        public string StatusClass => GetStatusClass(Status);
        public string ProgressBarClass => GetProgressBarClass();
        public decimal PercentComplete => Project?.PercentComplete ?? 0;

        public Project? Project { get; set; }
        public string Role { get; set; } = string.Empty;

        public static ProjectCardViewModel FromProject(Project project, string userRole)
        {
            return new ProjectCardViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Project = project,
                Role = userRole
            };
        }

        private string GetStatusClass(ProjectStatus status) => status switch
        {
            ProjectStatus.Draft => "bg-gray-100 text-gray-800",
            ProjectStatus.Active => "bg-blue-100 text-blue-800",
            ProjectStatus.Rejected => "bg-red-100 text-red-800",
            ProjectStatus.Completed => "bg-green-100 text-green-800",
            _ => "bg-gray-100 text-gray-800"
        };

        private string GetProgressBarClass()
        {
            var percent = PercentComplete;
            return percent switch
            {
                >= 75 => "bg-green-600",
                >= 50 => "bg-blue-600",
                >= 25 => "bg-yellow-600",
                _ => "bg-red-600"
            };
        }
    }

    public class TeamMemberViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int AvatarColorIndex { get; set; }

        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return "?";

                var parts = Name.Split(' ');
                if (parts.Length == 1)
                    return Name.Substring(0, Math.Min(2, Name.Length)).ToUpper();

                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            }
        }

        public string AvatarClass => AvatarColorIndex switch
        {
            0 => "bg-primary-600",
            1 => "bg-green-600",
            2 => "bg-purple-600",
            3 => "bg-orange-600",
            4 => "bg-pink-600",
            5 => "bg-blue-600",
            _ => "bg-primary-600"
        };
    }
}