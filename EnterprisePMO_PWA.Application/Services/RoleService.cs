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
    /// Provides methods for managing global roles.
    /// </summary>
    public class RoleService
    {
        private readonly AppDbContext _context;

        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        public async Task<Role> CreateRoleAsync(Role role)
        {
            role.Id = Guid.NewGuid();
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        /// <summary>
        /// Retrieves all roles.
        /// </summary>
        public List<Role> GetAllRoles()
        {
            return _context.Roles.ToList();
        }

        /// <summary>
        /// Updates an existing role.
        /// </summary>
        public async Task<Role?> UpdateRoleAsync(Role role)
        {
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return role;
        }

        /// <summary>
        /// Deletes a role.
        /// </summary>
        public async Task DeleteRoleAsync(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }
        }
    }
}
