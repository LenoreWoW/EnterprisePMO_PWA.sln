using System.Collections.Generic;
using System.Linq;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.ViewComponents
{
    /// <summary>
    /// View component to visualize role hierarchy.
    /// </summary>
    public class RoleHierarchyVisualizerViewComponent : ViewComponent
    {
        private readonly RoleService _roleService;

        public RoleHierarchyVisualizerViewComponent(RoleService roleService)
        {
            _roleService = roleService;
        }

        public IViewComponentResult Invoke()
        {
            var roles = _roleService.GetAllRoles()
                .OrderByDescending(r => r.HierarchyLevel)
                .ToList();
                
            return View(roles);
        }
    }
}