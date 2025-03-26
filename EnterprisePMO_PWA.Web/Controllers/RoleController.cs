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
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleService _roleService;
        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        public IActionResult List()
        {
            var roles = _roleService.GetAllRoles();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                // Get current user ID
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }
                
                await _roleService.CreateRoleAsync(role, userId.Value);
                return RedirectToAction("List");
            }
            return View(role);
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var role = _roleService.GetAllRoles().FirstOrDefault(r => r.Id == id);
            if (role == null)
                return NotFound();
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Role role)
        {
            if (ModelState.IsValid)
            {
                // Get current user ID
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized();
                }
                
                await _roleService.UpdateRoleAsync(role, userId.Value);
                return RedirectToAction("List");
            }
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Get current user ID
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }
            
            await _roleService.DeleteRoleAsync(id, userId.Value);
            return RedirectToAction("List");
        }
        
        /// <summary>
        /// Gets the current user's ID from claims.
        /// </summary>
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            
            return null;
        }
    }
}