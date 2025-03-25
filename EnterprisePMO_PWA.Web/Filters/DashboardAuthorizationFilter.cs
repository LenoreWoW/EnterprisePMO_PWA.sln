using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace EnterprisePMO_PWA.Web.Filters
{
    /// <summary>
    /// Custom authorization filter for the Hangfire Dashboard.
    /// Requires an admin key passed via query string.
    /// </summary>
    public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _adminKey;
        public DashboardAuthorizationFilter(IConfiguration configuration)
        {
            _adminKey = configuration["DashboardAdminKey"] ?? "default_admin_key";
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var queryKey = httpContext.Request.Query["adminKey"].ToString();
            return !string.IsNullOrEmpty(queryKey) && queryKey == _adminKey;
        }
    }
}
