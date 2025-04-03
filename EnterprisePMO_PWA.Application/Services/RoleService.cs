using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Provides methods for managing global roles with hierarchical permissions.
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public RoleService(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        public async Task<Role> CreateRoleAsync(Role role, Guid createdBy)
        {
            role.Id = Guid.NewGuid();
            role.CreatedAt = DateTime.UtcNow;
            role.UpdatedAt = DateTime.UtcNow;
            role.IsActive = true;

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            await _authService.LogActionAsync(
                "System",
                "Role Created",
                "Role",
                $"Created role: {role.RoleName}");

            return role;
        }

        /// <summary>
        /// Retrieves all roles.
        /// </summary>
        public IEnumerable<Role> GetAllRoles()
        {
            return _context.Roles
                .OrderBy(r => r.HierarchyLevel)
                .ToList();
        }

        /// <summary>
        /// Gets all roles that can be managed by a user with the specified role type.
        /// </summary>
        public IEnumerable<Role> GetManageableRoles(UserRole userRole)
        {
            var userHierarchyLevel = GetRoleHierarchyLevel(userRole);
            return _context.Roles
                .Where(r => r.HierarchyLevel < userHierarchyLevel)
                .OrderBy(r => r.HierarchyLevel)
                .ToList();
        }

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        public async Task<Role> UpdateRoleAsync(Role role, Guid updatedBy)
        {
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == role.Id);

            if (existingRole == null)
                return null;

            existingRole.RoleName = role.RoleName;
            existingRole.Description = role.Description;
            existingRole.HierarchyLevel = role.HierarchyLevel;
            existingRole.Type = role.Type;
            existingRole.CanManageProjects = role.CanManageProjects;
            existingRole.CanManageUsers = role.CanManageUsers;
            existingRole.CanApproveRequests = role.CanApproveRequests;
            existingRole.CanManageRoles = role.CanManageRoles;
            existingRole.CanViewReports = role.CanViewReports;
            existingRole.CanViewAuditLogs = role.CanViewAuditLogs;
            existingRole.InheritsPermissions = role.InheritsPermissions;
            existingRole.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _authService.LogActionAsync(
                "System",
                "Role Updated",
                "Role",
                $"Updated role: {role.RoleName}");

            return existingRole;
        }

        /// <summary>
        /// Deletes a role.
        /// </summary>
        public async Task<bool> DeleteRoleAsync(Guid roleId, Guid deletedBy)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
                return false;

            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _authService.LogActionAsync(
                "System",
                "Role Deleted",
                "Role",
                $"Deleted role: {role.RoleName}");

            return true;
        }
        
        /// <summary>
        /// Gets the hierarchy level for a role type.
        /// </summary>
        private int GetRoleHierarchyLevel(UserRole roleType)
        {
            switch (roleType)
            {
                case UserRole.Admin:
                    return 100;
                case UserRole.MainPMO:
                    return 90;
                case UserRole.Executive:
                    return 80;
                case UserRole.DepartmentDirector:
                    return 70;
                case UserRole.SubPMO:
                    return 60;
                case UserRole.ProjectManager:
                    return 50;
                default:
                    return 10;
            }
        }

        public async Task<IEnumerable<User>> GetUsersInRoleAsync(UserRole role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .ToListAsync();
        }

        public async Task<bool> IsInRoleAsync(Guid userId, UserRole role)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.Role == role;
        }

        public async Task<bool> AssignRoleAsync(Guid userId, UserRole role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.Role = role;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromRoleAsync(Guid userId, UserRole role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role != role)
                return false;

            user.Role = UserRole.Viewer; // Default to Viewer role
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserRole>> GetManageableRolesAsync(Guid userId)
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null)
                return Enumerable.Empty<UserRole>();

            var manageableRoles = new List<UserRole>();

            switch (currentUser.Role)
            {
                case UserRole.Admin:
                    manageableRoles.AddRange(new[]
                    {
                        UserRole.PMOHead,
                        UserRole.ProjectManager,
                        UserRole.TeamMember,
                        UserRole.Viewer
                    });
                    break;

                case UserRole.PMOHead:
                    manageableRoles.AddRange(new[]
                    {
                        UserRole.ProjectManager,
                        UserRole.TeamMember,
                        UserRole.Viewer
                    });
                    break;

                case UserRole.ProjectManager:
                    manageableRoles.AddRange(new[]
                    {
                        UserRole.TeamMember,
                        UserRole.Viewer
                    });
                    break;
            }

            return manageableRoles;
        }

        public async Task<bool> CanManageRoleAsync(Guid managerId, UserRole roleToManage)
        {
            var manageableRoles = await GetManageableRolesAsync(managerId);
            return manageableRoles.Contains(roleToManage);
        }
    }
}