using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize(Policy = "Permission:ManageRoles")]
    public class RoleManagementController : Controller
    {
        private readonly RoleService _roleService;
        private readonly PermissionService _permissionService;

        public RoleManagementController(RoleService roleService, PermissionService permissionService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var roles = _roleService.GetAllRoles();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Role());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                await _roleService.CreateRoleAsync(role, Guid.Parse(User.Identity?.Name ?? Guid.Empty.ToString()));
                return RedirectToAction(nameof(Index));
            }

            return View(role);
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var role = _roleService.GetAllRoles().FirstOrDefault(r => r.Id == id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Role role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _roleService.UpdateRoleAsync(role, Guid.Parse(User.Identity?.Name ?? Guid.Empty.ToString()));
                return RedirectToAction(nameof(Index));
            }

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _roleService.DeleteRoleAsync(id, Guid.Parse(User.Identity?.Name ?? Guid.Empty.ToString()));
            return RedirectToAction(nameof(Index));
        }

        private UserRole GetCurrentUserRoleType()
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Viewer;
        }

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
}