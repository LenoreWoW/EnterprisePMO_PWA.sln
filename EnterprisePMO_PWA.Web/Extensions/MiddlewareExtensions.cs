using Microsoft.AspNetCore.Builder;
using EnterprisePMO_PWA.Web.Middleware;

namespace EnterprisePMO_PWA.Web.Extensions
{
    /// <summary>
    /// Extensions for registering custom middleware
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds the authentication synchronization middleware to the application pipeline
        /// </summary>
        public static IApplicationBuilder UseAuthenticationSync(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}