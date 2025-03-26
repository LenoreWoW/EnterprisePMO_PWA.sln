using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace EnterprisePMO_PWA.Web.Authorization
{
    /// <summary>
    /// Authorization requirement for permissions.
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    /// <summary>
    /// Authorization handler for permission requirements.
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Skip if user is not authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return;
            }

            // Get user ID from claims
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return;
            }

            // Using a scope to resolve scoped services in singleton handler
            using (var scope = _serviceProvider.CreateScope())
            {
                var permissionService = scope.ServiceProvider.GetRequiredService<PermissionService>();
                
                // Check if user has the required permission
                if (await permissionService.HasPermissionAsync(userId, requirement.Permission))
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}