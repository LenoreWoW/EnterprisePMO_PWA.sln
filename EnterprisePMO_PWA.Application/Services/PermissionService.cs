using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Services;
using EnterprisePMO_PWA.Infrastructure.Data;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Provides methods for checking user permissions.
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checks if a user has a specific permission.
        /// </summary>
        public async Task<bool> HasPermissionAsync(Guid userId, string permission)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            // Handle global-level permissions based on the user's role type
            switch (permission)
            {
                case "ManageProjects":
                    return user.Role == RoleType.ProjectManager || 
                           user.Role == RoleType.SubPMO || 
                           user.Role == RoleType.MainPMO || 
                           user.Role == RoleType.Admin;

                case "ManageUsers":
                    return user.Role == RoleType.MainPMO || 
                           user.Role == RoleType.Admin;

                case "ApproveRequests":
                    return user.Role == RoleType.SubPMO || 
                           user.Role == RoleType.MainPMO || 
                           user.Role == RoleType.Admin;

                case "ManageRoles":
                    return user.Role == RoleType.Admin;

                case "ViewReports":
                    return user.Role == RoleType.ProjectManager || 
                           user.Role == RoleType.SubPMO || 
                           user.Role == RoleType.MainPMO || 
                           user.Role == RoleType.DepartmentDirector || 
                           user.Role == RoleType.Executive || 
                           user.Role == RoleType.Admin;

                case "ViewAuditLogs":
                    return user.Role == RoleType.MainPMO || 
                           user.Role == RoleType.Admin;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if a user can access a project.
        /// </summary>
        public async Task<bool> CanAccessProjectAsync(Guid userId, Guid projectId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            // Admin and Main PMO have access to all projects
            if (user.Role == RoleType.Admin || user.Role == RoleType.MainPMO)
                return true;

            // Executives have access to all projects
            if (user.Role == RoleType.Executive)
                return true;

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return false;

            // Project Manager has access to their own projects
            if (user.Role == RoleType.ProjectManager && project.ProjectManagerId == userId)
                return true;

            // Department Director has access to projects in their department
            if (user.Role == RoleType.DepartmentDirector && user.DepartmentId.HasValue && 
                project.DepartmentId == user.DepartmentId.Value)
                return true;

            // Sub PMO has access to projects in their department
            if (user.Role == RoleType.SubPMO && user.DepartmentId.HasValue && 
                project.DepartmentId == user.DepartmentId.Value)
                return true;

            // Check if user is a project member
            var isMember = await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);

            return isMember;
        }

        /// <summary>
        /// Checks if a user can manage a specific project.
        /// </summary>
        public async Task<bool> CanManageProjectAsync(Guid userId, Guid projectId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            // Admin and Main PMO can manage all projects
            if (user.Role == RoleType.Admin || user.Role == RoleType.MainPMO)
                return true;

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return false;

            // Project Manager can manage their own projects
            if (user.Role == RoleType.ProjectManager && project.ProjectManagerId == userId)
                return true;

            // Sub PMO can manage projects in their department
            if (user.Role == RoleType.SubPMO && user.DepartmentId.HasValue && 
                project.DepartmentId == user.DepartmentId.Value)
                return true;

            return false;
        }
    }
}