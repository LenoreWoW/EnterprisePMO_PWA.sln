using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
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
            // Get the current user's role type
            var userRole = GetCurrentUserRoleType();
            
            // Get roles that can be managed by this role
            var roles = _roleService.GetManageableRoles(userRole);
            
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Pass the user's role in ViewBag to limit hierarchy level in view
            ViewBag.CurrentUserRole = GetCurrentUserRoleType();
            
            return View(new Role { InheritsPermissions = true });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            // Validate that the user can't create a role with higher hierarchy level
            var userRole = GetCurrentUserRoleType();
            int userHierarchyLevel = GetRoleHierarchyLevel(userRole);
            
            if (role.HierarchyLevel >= userHierarchyLevel)
            {
                ModelState.AddModelError("HierarchyLevel", "You cannot create a role with equal or higher hierarchy level than your own.");
                ViewBag.CurrentUserRole = userRole;
                return View(role);
            }
            
            if (ModelState.IsValid)
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    await _roleService.CreateRoleAsync(role, userId);
                    return RedirectToAction("Index");
                }
            }
            
            ViewBag.CurrentUserRole = userRole;
            return View(role);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            // Get the role
            var roles = _roleService.GetAllRoles();
            var role = roles.FirstOrDefault(r => r.Id == id);
            
            if (role == null)
                return NotFound();
                
            // Check if current user can manage this role
            var userRole = GetCurrentUserRoleType();
            int userHierarchyLevel = GetRoleHierarchyLevel(userRole);
            
            if (role.HierarchyLevel >= userHierarchyLevel)
            {
                return Forbid(); // User cannot edit a role with equal or higher hierarchy
            }
            
            ViewBag.CurrentUserRole = userRole;
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Role role)
        {
            // Validate that the user can't edit a role to have higher hierarchy level
            var userRole = GetCurrentUserRoleType();
            int userHierarchyLevel = GetRoleHierarchyLevel(userRole);
            
            if (role.HierarchyLevel >= userHierarchyLevel)
            {
                ModelState.AddModelError("HierarchyLevel", "You cannot set a hierarchy level equal or higher than your own.");
                ViewBag.CurrentUserRole = userRole;
                return View(role);
            }
            
            if (ModelState.IsValid)
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    await _roleService.UpdateRoleAsync(role, userId);
                    return RedirectToAction("Index");
                }
            }
            
            ViewBag.CurrentUserRole = userRole;
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Get the role
            var roles = _roleService.GetAllRoles();
            var role = roles.FirstOrDefault(r => r.Id == id);
            
            if (role == null)
                return NotFound();
                
            // Check if current user can manage this role
            var userRole = GetCurrentUserRoleType();
            int userHierarchyLevel = GetRoleHierarchyLevel(userRole);
            
            if (role.HierarchyLevel >= userHierarchyLevel)
            {
                return Forbid(); // User cannot delete a role with equal or higher hierarchy
            }
            
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                await _roleService.DeleteRoleAsync(id, userId);
            }
            
            return RedirectToAction("Index");
        }
        
        #region Helper Methods
        
        private RoleType GetCurrentUserRoleType()
        {
            var roleValue = User.FindFirst(ClaimTypes.Role)?.Value;
            if (Enum.TryParse<RoleType>(roleValue, out var role))
            {
                return role;
            }
            
            return RoleType.ProjectManager; // Default role if not found
        }
        
        private int GetRoleHierarchyLevel(RoleType roleType)
        {
            switch (roleType)
            {
                case RoleType.Admin:
                    return 100;
                case RoleType.MainPMO:
                    return 90;
                case RoleType.Executive:
                    return 80;
                case RoleType.DepartmentDirector:
                    return 70;
                case RoleType.SubPMO:
                    return 60;
                case RoleType.ProjectManager:
                    return 50;
                default:
                    return 10;
            }
        }
        
        #endregion
    }
}