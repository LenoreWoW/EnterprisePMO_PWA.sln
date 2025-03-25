using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize]
    public class WeeklyUpdatesController : Controller
    {
        private readonly WeeklyUpdateService _weeklyUpdateService;
        public WeeklyUpdatesController(WeeklyUpdateService weeklyUpdateService)
        {
            _weeklyUpdateService = weeklyUpdateService;
        }

        [Authorize(Roles = "ProjectManager,SubPMO,MainPMO")]
        public IActionResult List(Guid projectId)
        {
            // TODO: Retrieve weekly updates for the specified project.
            ViewBag.ProjectId = projectId;
            return View();
        }

        [Authorize(Roles = "ProjectManager")]
        [HttpGet]
        public IActionResult Create(Guid projectId)
        {
            ViewBag.ProjectId = projectId;
            return View();
        }

        [Authorize(Roles = "ProjectManager")]
        [HttpPost]
        public async Task<IActionResult> Create(WeeklyUpdate update)
        {
            if (ModelState.IsValid)
            {
                await _weeklyUpdateService.SubmitWeeklyUpdateAsync(update);
                return RedirectToAction("List", new { projectId = update.ProjectId });
            }
            return View(update);
        }

        [Authorize(Roles = "ProjectManager")]
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var update = _weeklyUpdateService.GetWeeklyUpdateById(id);
            if (update == null) return NotFound();
            return View(update);
        }

        [Authorize(Roles = "ProjectManager")]
        [HttpPost]
        public async Task<IActionResult> Edit(WeeklyUpdate update)
        {
            if (ModelState.IsValid)
            {
                await _weeklyUpdateService.UpdateWeeklyUpdateAsync(update);
                return RedirectToAction("List", new { projectId = update.ProjectId });
            }
            return View(update);
        }

        [Authorize(Roles = "SubPMO")]
        [HttpPost]
        public async Task<IActionResult> Approve(Guid id)
        {
            await _weeklyUpdateService.ApproveWeeklyUpdateAsync(id);
            return RedirectToAction("List");
        }

        [Authorize(Roles = "SubPMO")]
        [HttpPost]
        public async Task<IActionResult> Reject(Guid id)
        {
            await _weeklyUpdateService.RejectWeeklyUpdateAsync(id);
            return RedirectToAction("List");
        }
    }
}
