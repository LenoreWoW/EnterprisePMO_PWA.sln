using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using EnterprisePMO_PWA.Domain.Authorization;

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
            private UserRole? _roleType;

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
            public bool HasHigherRoleThan(UserRole roleType)
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
            public bool HasRole(UserRole roleType)
            {
                var currentRole = GetRoleType();
                return currentRole.HasValue && currentRole.Value == roleType;
            }

            /// <summary>
            /// Gets the global role of the current user.
            /// </summary>
            public UserRole? GetRoleType()
            {
                if (_roleType.HasValue)
                {
                    return _roleType;
                }

                var roleValue = _httpContext.User.FindFirst(ClaimTypes.Role)?.Value;
                if (Enum.TryParse<UserRole>(roleValue, out var role))
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
        }

        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleClaim))
                return false;

            if (Enum.TryParse<UserRole>(roleClaim, out var userRole))
            {
                return Permissions.RolePermissions.TryGetValue(userRole, out var permissions) &&
                       permissions.Contains(permission);
            }

            return false;
        }

        public static bool HasAnyPermission(this ClaimsPrincipal user, params string[] permissions)
        {
            return permissions.Any(permission => HasPermission(user, permission));
        }

        public static bool HasAllPermissions(this ClaimsPrincipal user, params string[] permissions)
        {
            return permissions.All(permission => HasPermission(user, permission));
        }

        public static UserRole GetUserRole(this ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Viewer;
        }
    }

    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string permission) : base(typeof(RequirePermissionFilter))
        {
            Arguments = new object[] { permission };
        }
    }

    public class RequirePermissionFilter : IAuthorizationFilter
    {
        private readonly string _permission;

        public RequirePermissionFilter(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.HasPermission(_permission))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}