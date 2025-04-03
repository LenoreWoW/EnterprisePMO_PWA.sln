using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterprisePMO_PWA.Application.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(Guid userId, string permission);
        Task<bool> HasAnyPermissionAsync(Guid userId, IEnumerable<string> permissions);
        Task<bool> HasAllPermissionsAsync(Guid userId, IEnumerable<string> permissions);
    }
} 