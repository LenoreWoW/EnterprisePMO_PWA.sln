using System;
using System.Threading.Tasks;

namespace EnterprisePMO_PWA.Domain.Services
{
    /// <summary>
    /// Defines methods for checking user permissions.
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Checks if a user has a specific permission.
        /// </summary>
        Task<bool> HasPermissionAsync(Guid userId, string permission);

        /// <summary>
        /// Checks if a user can access a project.
        /// </summary>
        Task<bool> CanAccessProjectAsync(Guid userId, Guid projectId);

        /// <summary>
        /// Checks if a user can manage a specific project.
        /// </summary>
        Task<bool> CanManageProjectAsync(Guid userId, Guid projectId);
    }
}