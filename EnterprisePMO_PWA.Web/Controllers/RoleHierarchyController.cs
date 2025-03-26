using System.Linq;
using EnterprisePMO_PWA.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.Controllers
{
    /// <summary>
    /// Controller for displaying role hierarchy.
    /// </summary>
    [Authorize]
    public class RoleHierarchyController : Controller
    {
        private readonly RoleService _roleService;

        public RoleHierarchyController(RoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Displays the role hierarchy visualization.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            var roles = _roleService.GetAllRoles()
                .OrderByDescending(r => r.HierarchyLevel)
                .ToList();
                
            return View(roles);
        }
    }
}