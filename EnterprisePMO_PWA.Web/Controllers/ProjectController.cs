using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ProjectService _projectService;
        public ProjectController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        [Authorize(Roles = "ProjectManager,SubPMO,MainPMO,DepartmentDirector,Executive")]
        public IActionResult List() => View(); // Data loaded via view model or API

        [Authorize(Roles = "ProjectManager")]
        [HttpGet]
        public IActionResult Create() => View();

        [Authorize(Roles = "ProjectManager")]
        [HttpPost]
        public async Task<IActionResult> Create(Project project)
        {
            if (ModelState.IsValid)
            {
                await _projectService.CreateProjectAsync(project);
                return RedirectToAction("List");
            }
            return View(project);
        }

        [Authorize(Roles = "ProjectManager")]
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var project = _projectService.GetProjectById(id);
            if (project == null) return NotFound();
            return View(project);
        }

        [Authorize(Roles = "ProjectManager")]
        [HttpPost]
        public async Task<IActionResult> Edit(Project project)
        {
            if (ModelState.IsValid)
            {
                await _projectService.UpdateProjectAsync(project);
                return RedirectToAction("List");
            }
            return View(project);
        }

        [Authorize(Roles = "ProjectManager")]
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _projectService.DeleteProjectAsync(id);
            return RedirectToAction("List");
        }

        [Authorize(Roles = "ProjectManager,SubPMO,MainPMO,DepartmentDirector,Executive")]
        public IActionResult Details(Guid id)
        {
            var project = _projectService.GetProjectById(id);
            if (project == null) return NotFound();
            return View(project);
        }
    }
}
