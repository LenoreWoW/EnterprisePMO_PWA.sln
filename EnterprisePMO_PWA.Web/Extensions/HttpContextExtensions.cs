using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;

namespace EnterprisePMO_PWA.Web.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Signs out the current user
        /// </summary>
        public static async Task SignOut(this HttpContext context)
        {
            await context.SignOutAsync();
        }

        /// <summary>
        /// Signs in a user with a JWT token
        /// </summary>
        public static async Task SignInWithJwtToken(this HttpContext context, string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(securityToken?.Claims, "JWT"));
            await context.SignInAsync(claimsPrincipal);
        }

        /// <summary>
        /// Gets the current user's ID from claims
        /// </summary>
        public static Guid? GetCurrentUserId(this HttpContext context)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier) ?? 
                              context.User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }

            return null;
        }

        /// <summary>
        /// Gets the current user's username from claims
        /// </summary>
        public static string? GetCurrentUsername(this HttpContext context)
        {
            return context.User.Identity?.Name;
        }

        /// <summary>
        /// Gets the current user's role from claims
        /// </summary>
        public static string? GetCurrentUserRole(this HttpContext context)
        {
            return context.User.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        public static bool IsInRole(this HttpContext context, string role)
        {
            return context.User.IsInRole(role);
        }
    }
}