using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EnterprisePMO_PWA.Web.Helpers
{
    /// <summary>
    /// Helper class to check user permissions throughout the application.
    /// </summary>
    public static class PermissionHelper
    {
        /// <summary>
        /// Creates a helper instance for the current request.
        /// </summary>
        public static PermissionHelperInstance For(HttpContext httpContext, IServiceProvider serviceProvider)
        {
            return new PermissionHelperInstance(httpContext, serviceProvider);
        }

        /// <summary>
        /// Instance class to check permissions for a specific HTTP context.
        /// </summary>
        public class PermissionHelperInstance
        {
            private readonly HttpContext _httpContext;
            private readonly IServiceProvider _serviceProvider;
            private Guid? _userId;
            private RoleType? _roleType;

            public PermissionHelperInstance(HttpContext httpContext, IServiceProvider serviceProvider)
            {
                _httpContext = httpContext;
                _serviceProvider = serviceProvider;
            }

            /// <summary>
            /// Checks if the current user has the specified permission.
            /// </summary>
            public async Task<bool> HasPermission(string permissionName)
            {
                if (!_httpContext.User.Identity?.IsAuthenticated ?? true)
                {
                    return false;
                }

                // Get the permission service
                using var scope = _serviceProvider.CreateScope();
                var permissionService = scope.ServiceProvider.GetRequiredService<PermissionService>();

                // Get user ID
                var userId = GetUserId();
                if (!userId.HasValue)
                {
                    return false;
                }

                return await permissionService.HasPermissionAsync(userId.Value, permissionName);
            }

            /// <summary>
            /// Checks if the current user has a higher role level than the specified role.
            /// </summary>
            public bool HasHigherRoleThan(RoleType roleType)
            {
                var currentRole = GetRoleType();
                if (!currentRole.HasValue)
                {
                    return false;
                }

                int currentLevel = GetRoleHierarchyLevel(currentRole.Value);
                int targetLevel = GetRoleHierarchyLevel(roleType);

                return currentLevel > targetLevel;
            }

            /// <summary>
            /// Checks if the current user has the specified global role.
            /// </summary>
            public bool HasRole(RoleType roleType)
            {
                var currentRole = GetRoleType();
                return currentRole.HasValue && currentRole.Value == roleType;
            }

            /// <summary>
            /// Gets the global role of the current user.
            /// </summary>
            public RoleType? GetRoleType()
            {
                if (_roleType.HasValue)
                {
                    return _roleType;
                }

                var roleValue = _httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                if (Enum.TryParse<RoleType>(roleValue, out var role))
                {
                    _roleType = role;
                    return role;
                }

                return null;
            }

            /// <summary>
            /// Gets the ID of the current user.
            /// </summary>
            public Guid? GetUserId()
            {
                if (_userId.HasValue)
                {
                    return _userId;
                }

                var userIdClaim = _httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    _userId = userId;
                    return userId;
                }

                return null;
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
}