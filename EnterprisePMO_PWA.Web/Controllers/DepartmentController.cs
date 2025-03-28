using System;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
        private readonly DepartmentService _departmentService;
        private readonly AppDbContext _context;

        public DepartmentController(DepartmentService departmentService, AppDbContext context)
        {
            _departmentService = departmentService;
            _context = context;
        }

        // MVC Actions

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

        // API Endpoints

        [HttpGet]
        [Route("api/departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Where(d => d.Name != "Holding") // Exclude Holding department from signup
                .Select(d => new { d.Id, d.Name })
                .OrderBy(d => d.Name)
                .ToListAsync();

            return Ok(departments);
        }
    }
}