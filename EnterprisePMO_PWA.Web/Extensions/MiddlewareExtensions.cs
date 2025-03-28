using Microsoft.AspNetCore.Builder;

namespace EnterprisePMO_PWA.Web.Extensions
{
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationSync(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EnterprisePMO_PWA.Web.Middleware.AuthenticationMiddleware>();
        }
    }
}