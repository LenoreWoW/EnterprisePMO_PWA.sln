using System;
using System.Linq;
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
                await _roleService.CreateRoleAsync(role);
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
                await _roleService.UpdateRoleAsync(role);
                return RedirectToAction("List");
            }
            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _roleService.DeleteRoleAsync(id);
            return RedirectToAction("List");
        }
    }
}
