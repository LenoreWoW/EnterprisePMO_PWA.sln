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
    /// Provides methods for managing users in the system.
    /// </summary>
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public UserService(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        /// <summary>
        /// Gets all users in the system.
        /// </summary>
        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users
                .Include(u => u.Department)
                .OrderBy(u => u.Username)
                .ToList();
        }

        /// <summary>
        /// Gets a user by their ID.
        /// </summary>
        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        public async Task<User> CreateUserAsync(User user, Guid createdBy)
        {
            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsActive = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "User",
                user.Id,
                "Create",
                $"Created user: {user.Username}");

            return user;
        }

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        public async Task<User?> UpdateUserAsync(User user, Guid updatedBy)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                return null;
            }

            existingUser.Username = user.Username;
            existingUser.Role = user.Role;
            existingUser.DepartmentId = user.DepartmentId;
            existingUser.IsActive = user.IsActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "User",
                user.Id,
                "Update",
                $"Updated user: {user.Username}");

            return existingUser;
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        public async Task<bool> DeleteUserAsync(Guid id, Guid deletedBy)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await _auditService.LogActionAsync(
                "User",
                id,
                "Delete",
                $"Deleted user: {user.Username}");

            return true;
        }
    }
} 