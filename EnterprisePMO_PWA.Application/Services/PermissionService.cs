using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Services;
using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Domain.Enums;
using EnterprisePMO_PWA.Domain.Authorization;

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

        public IEnumerable<string> GetUserPermissions(UserRole userRole)
        {
            if (Permissions.RolePermissions.TryGetValue(userRole, out var permissions))
            {
                return permissions;
            }
            return Array.Empty<string>();
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

            return Permissions.RolePermissions.TryGetValue(user.Role, out var permissions) &&
                   permissions.Contains(permission);
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
            if (user.Role == UserRole.Admin || user.Role == UserRole.MainPMO)
                return true;

            // Executives have access to all projects
            if (user.Role == UserRole.Executive)
                return true;

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return false;

            // Project Manager has access to their own projects
            if (user.Role == UserRole.ProjectManager && project.ProjectManagerId == userId)
                return true;

            // Department Director has access to projects in their department
            if (user.Role == UserRole.DepartmentDirector && 
                project.DepartmentId == user.DepartmentId)
                return true;

            // Sub PMO has access to projects in their department
            if (user.Role == UserRole.SubPMO && 
                project.DepartmentId == user.DepartmentId)
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
            if (user.Role == UserRole.Admin || user.Role == UserRole.MainPMO)
                return true;

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return false;

            // Project Manager can manage their own projects
            if (user.Role == UserRole.ProjectManager && project.ProjectManagerId == userId)
                return true;

            // Sub PMO can manage projects in their department
            if (user.Role == UserRole.SubPMO && 
                project.DepartmentId == user.DepartmentId)
                return true;

            return false;
        }

        public async Task<bool> HasAnyPermissionAsync(Guid userId, IEnumerable<string> permissions)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            return Permissions.RolePermissions.TryGetValue(user.Role, out var rolePermissions) &&
                   permissions.Any(p => rolePermissions.Contains(p));
        }

        public async Task<bool> HasAllPermissionsAsync(Guid userId, IEnumerable<string> permissions)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            return Permissions.RolePermissions.TryGetValue(user.Role, out var rolePermissions) &&
                   permissions.All(p => rolePermissions.Contains(p));
        }

        private bool HasPermissionForRole(UserRole role, string permission)
        {
            // Admin has all permissions
            if (role == UserRole.Admin)
                return true;

            // PMO Head has all project-related permissions
            if (role == UserRole.PMOHead)
            {
                return permission.StartsWith("Projects.") ||
                       permission.StartsWith("Reports.") ||
                       permission.StartsWith("Users.") ||
                       permission.StartsWith("Departments.");
            }

            // Project Manager has project management permissions
            if (role == UserRole.ProjectManager)
            {
                return permission.StartsWith("Projects.") ||
                       permission.StartsWith("Reports.View") ||
                       permission.StartsWith("Tasks.");
            }

            // Team Member has task-related permissions
            if (role == UserRole.TeamMember)
            {
                return permission.StartsWith("Tasks.") ||
                       permission.StartsWith("Projects.View") ||
                       permission.StartsWith("Reports.View");
            }

            // Viewer has view-only permissions
            if (role == UserRole.Viewer)
            {
                return permission.StartsWith("Projects.View") ||
                       permission.StartsWith("Reports.View");
            }

            return false;
        }
    }
}