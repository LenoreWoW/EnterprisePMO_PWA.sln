using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize(Roles = "ProjectManager")]
    public class ProjectRolesController : Controller
    {
        private readonly ProjectMemberService _projectMemberService;
        public ProjectRolesController(ProjectMemberService projectMemberService)
        {
            _projectMemberService = projectMemberService;
        }

        [HttpGet]
        public IActionResult Assign(Guid projectId)
        {
            ViewBag.ProjectId = projectId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Assign(Guid projectId, Guid userId, string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                ModelState.AddModelError("role", "Role is required.");
                ViewBag.ProjectId = projectId;
                return View();
            }
            await _projectMemberService.AddMemberAsync(projectId, userId, role);
            return RedirectToAction("Details", "Project", new { id = projectId });
        }
    }
}
