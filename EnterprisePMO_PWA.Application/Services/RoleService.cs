using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Provides methods for managing global roles with hierarchical permissions.
    /// </summary>
    public class RoleService
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public RoleService(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        public async Task<Role> CreateRoleAsync(Role role, Guid createdByUserId)
        {
            role.Id = Guid.NewGuid();
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            
            // Log the action
            await _auditService.LogActionAsync(
                "Role",
                role.Id,
                "Create",
                $"Role '{role.RoleName}' created with hierarchy level {role.HierarchyLevel}"
            );
            
            return role;
        }

        /// <summary>
        /// Retrieves all roles.
        /// </summary>
        public List<Role> GetAllRoles()
        {
            return _context.Roles
                .OrderByDescending(r => r.HierarchyLevel)
                .ToList();
        }

        /// <summary>
        /// Gets all roles that can be managed by a user with the specified role type.
        /// </summary>
        public List<Role> GetManageableRoles(RoleType userRoleType)
        {
            int hierarchyLevel = GetRoleHierarchyLevel(userRoleType);
            
            return _context.Roles
                .Where(r => r.HierarchyLevel < hierarchyLevel)
                .OrderByDescending(r => r.HierarchyLevel)
                .ToList();
        }

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        public async Task<Role?> UpdateRoleAsync(Role role, Guid updatedByUserId)
        {
            // Retrieve the original role for audit comparison
            var originalRole = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == role.Id);
            if (originalRole == null)
                return null;
                
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            // Log the action with changes
            await _auditService.LogActionAsync(
                "Role",
                role.Id,
                "Update",
                _auditService.CreateChangeSummary(originalRole, role)
            );
            
            return role;
        }

        /// <summary>
        /// Deletes a role.
        /// </summary>
        public async Task DeleteRoleAsync(Guid id, Guid deletedByUserId)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                
                // Log the action
                await _auditService.LogActionAsync(
                    "Role",
                    id,
                    "Delete",
                    $"Role '{role.RoleName}' with hierarchy level {role.HierarchyLevel} was deleted"
                );
            }
        }
        
        /// <summary>
        /// Gets the hierarchy level for a role type.
        /// </summary>
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
    }
}