using System;
using System.Collections.Generic;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;

namespace EnterprisePMO_PWA.Domain.Authorization
{
    public static class Permissions
    {
        public const string ExportReports = "Reports.Export";

        public static readonly Dictionary<UserRole, HashSet<string>> RolePermissions = new()
        {
            {
                UserRole.Admin,
                new HashSet<string>
                {
                    "Projects.View",
                    "Projects.Create",
                    "Projects.Edit",
                    "Projects.Delete",
                    "Projects.Approve",
                    "Users.View",
                    "Users.Create",
                    "Users.Edit",
                    "Users.Delete",
                    "Roles.View",
                    "Roles.Create",
                    "Roles.Edit",
                    "Roles.Delete",
                    "Departments.View",
                    "Departments.Create",
                    "Departments.Edit",
                    "Departments.Delete",
                    "Reports.View",
                    "Reports.Create",
                    "Reports.Edit",
                    "Reports.Delete",
                    ExportReports
                }
            },
            {
                UserRole.PMOHead,
                new HashSet<string>
                {
                    "Projects.View",
                    "Projects.Create",
                    "Projects.Edit",
                    "Projects.Approve",
                    "Users.View",
                    "Users.Create",
                    "Users.Edit",
                    "Reports.View",
                    "Reports.Create",
                    "Reports.Edit",
                    ExportReports
                }
            },
            {
                UserRole.ProjectManager,
                new HashSet<string>
                {
                    "Projects.View",
                    "Projects.Create",
                    "Projects.Edit",
                    "Reports.View",
                    "Reports.Create",
                    ExportReports
                }
            },
            {
                UserRole.TeamMember,
                new HashSet<string>
                {
                    "Projects.View",
                    "Reports.View",
                    ExportReports
                }
            },
            {
                UserRole.Viewer,
                new HashSet<string>
                {
                    "Projects.View",
                    "Reports.View"
                }
            }
        };
    }
} 