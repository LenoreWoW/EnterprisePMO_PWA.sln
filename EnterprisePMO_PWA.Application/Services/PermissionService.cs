using System;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Service for checking and managing user permissions based on roles.
    /// </summary>
    public class PermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checks if the user has a specific permission either directly or via role inheritance.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="permissionName">The name of the permission to check</param>
        /// <returns>True if the user has the permission, false otherwise</returns>
        public async Task<bool> HasPermissionAsync(Guid userId, string permissionName)
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            // Admin role has all permissions
            if (user.Role == RoleType.Admin)
                return true;

            // For project-specific roles, we'd need to check the project membership
            // This example just checks global roles
            
            // Find all roles that match the permission
            var roles = await _context.Roles.ToListAsync();
            
            // Check if the user's global role has the permission
            var hasPermission = false;
            
            switch (permissionName)
            {
                case "ManageProjects":
                    hasPermission = user.Role == RoleType.ProjectManager || 
                                    user.Role == RoleType.SubPMO || 
                                    user.Role == RoleType.MainPMO;
                    break;
                case "ApproveRequests":
                    hasPermission = user.Role == RoleType.SubPMO || 
                                    user.Role == RoleType.MainPMO;
                    break;
                case "ManageUsers":
                    hasPermission = user.Role == RoleType.Admin;
                    break;
                case "ViewReports":
                    hasPermission = user.Role == RoleType.ProjectManager || 
                                    user.Role == RoleType.SubPMO || 
                                    user.Role == RoleType.MainPMO || 
                                    user.Role == RoleType.DepartmentDirector || 
                                    user.Role == RoleType.Executive;
                    break;
                case "ViewAuditLogs":
                    hasPermission = user.Role == RoleType.Admin || 
                                    user.Role == RoleType.MainPMO;
                    break;
                default:
                    hasPermission = false;
                    break;
            }
            
            return hasPermission;
        }

        /// <summary>
        /// Checks if the user has a higher role in the hierarchy than the target user.
        /// </summary>
        /// <param name="userId">The user ID checking permissions</param>
        /// <param name="targetUserId">The target user ID</param>
        /// <returns>True if the user has a higher role, false otherwise</returns>
        public async Task<bool> HasHigherRoleAsync(Guid userId, Guid targetUserId)
        {
            var user = await _context.Users.FindAsync(userId);
            var targetUser = await _context.Users.FindAsync(targetUserId);

            if (user == null || targetUser == null)
                return false;

            // Admin is always highest
            if (user.Role == RoleType.Admin)
                return true;

            // Convert enum to hierarchy level
            int userLevel = GetRoleHierarchyLevel(user.Role);
            int targetLevel = GetRoleHierarchyLevel(targetUser.Role);

            return userLevel > targetLevel;
        }

        /// <summary>
        /// Gets the hierarchy level for a role type.
        /// </summary>
        /// <param name="roleType">The role type</param>
        /// <returns>The hierarchy level (higher = more permissions)</returns>
        private int GetRoleHierarchyLevel(RoleType roleType)
        {
            switch (roleType)
            {
                case RoleType.Admin:
                    return 100;
                case RoleType.MainPMO:
                    return 90;
                case RoleType.Executive:
                    return 80;
                case RoleType.DepartmentDirector:
                    return 70;
                case RoleType.SubPMO:
                    return 60;
                case RoleType.ProjectManager:
                    return 50;
                default:
                    return 10;
            }
        }

        /// <summary>
        /// Gets all roles that a given role can manage.
        /// </summary>
        /// <param name="roleType">The role type</param>
        /// <returns>Array of role types that can be managed</returns>
        public RoleType[] GetManageableRoles(RoleType roleType)
        {
            int level = GetRoleHierarchyLevel(roleType);
            
            // Can manage all roles with lower hierarchy level
            return Enum.GetValues<RoleType>()
                .Where(r => GetRoleHierarchyLevel(r) < level)
                .ToArray();
        }
    }
}