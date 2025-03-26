using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Web.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace EnterprisePMO_PWA.Web.TagHelpers
{
    /// <summary>
    /// Tag helper to conditionally show/hide elements based on user permissions.
    /// Usage: <div permission="ManageProjects">Only visible to users with ManageProjects permission</div>
    /// </summary>
    [HtmlTargetElement(Attributes = "permission")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public PermissionTagHelper(
            IServiceProvider serviceProvider,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets or sets the permission name to check.
        /// </summary>
        [HtmlAttributeName("permission")]
        public string Permission { get; set; } = string.Empty;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Permission))
            {
                output.SuppressOutput();
                return;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                output.SuppressOutput();
                return;
            }

            var permissionHelper = PermissionHelper.For(httpContext, _serviceProvider);
            bool hasPermission = await permissionHelper.HasPermission(Permission);

            if (!hasPermission)
            {
                output.SuppressOutput();
            }

            // Remove the permission attribute
            output.Attributes.RemoveAll("permission");
        }
    }
}