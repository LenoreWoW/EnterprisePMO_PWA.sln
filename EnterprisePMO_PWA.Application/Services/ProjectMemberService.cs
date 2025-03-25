using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Provides methods for managing project member assignments.
    /// </summary>
    public class ProjectMemberService
    {
        private readonly AppDbContext _context;

        public ProjectMemberService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a member to a project with a specified role.
        /// </summary>
        public async Task<ProjectMember> AddMemberAsync(Guid projectId, Guid userId, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                throw new Exception("Role not found.");
            }

            var member = new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = userId,
                RoleId = role.Id
            };

            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();
            return member;
        }

        /// <summary>
        /// Updates the role of an existing project member.
        /// </summary>
        public async Task<ProjectMember?> UpdateMemberRoleAsync(Guid memberId, string newRoleName)
        {
            var member = await _context.ProjectMembers.FirstOrDefaultAsync(m => m.Id == memberId);
            if (member == null)
                return null;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName.Equals(newRoleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
                throw new Exception("Role not found.");

            member.RoleId = role.Id;
            await _context.SaveChangesAsync();
            return member;
        }

        /// <summary>
        /// Deletes a project member.
        /// </summary>
        public async Task DeleteMemberAsync(Guid memberId)
        {
            var member = await _context.ProjectMembers.FirstOrDefaultAsync(m => m.Id == memberId);
            if (member != null)
            {
                _context.ProjectMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }
    }
}
