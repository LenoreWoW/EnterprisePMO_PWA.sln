using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;

namespace EnterprisePMO_PWA.Application.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<User>> GetUsersInRoleAsync(UserRole role);
        Task<bool> IsInRoleAsync(Guid userId, UserRole role);
        Task<bool> AssignRoleAsync(Guid userId, UserRole role);
        Task<bool> RemoveFromRoleAsync(Guid userId, UserRole role);
        Task<IEnumerable<UserRole>> GetManageableRolesAsync(Guid userId);
        Task<bool> CanManageRoleAsync(Guid managerId, UserRole roleToManage);
    }
} 