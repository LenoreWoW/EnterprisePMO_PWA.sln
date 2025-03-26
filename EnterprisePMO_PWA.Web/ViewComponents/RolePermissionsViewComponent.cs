using EnterprisePMO_PWA.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.ViewComponents
{
    /// <summary>
    /// View component to display role permissions.
    /// </summary>
    public class RolePermissionsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Role role)
        {
            return View(role);
        }
    }
}