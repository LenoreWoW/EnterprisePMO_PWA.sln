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
    public class DepartmentController : Controller
    {
        private readonly DepartmentService _departmentService;
        public DepartmentController(DepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        public IActionResult List()
        {
            var departments = _departmentService.GetAllDepartments();
            return View(departments);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                await _departmentService.CreateDepartmentAsync(department);
                return RedirectToAction("List");
            }
            return View(department);
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var dept = _departmentService.GetAllDepartments().FirstOrDefault(d => d.Id == id);
            if (dept == null) return NotFound();
            return View(dept);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Department department)
        {
            if (ModelState.IsValid)
            {
                await _departmentService.UpdateDepartmentAsync(department);
                return RedirectToAction("List");
            }
            return View(department);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _departmentService.DeleteDepartmentAsync(id);
            return RedirectToAction("List");
        }
    }
}
