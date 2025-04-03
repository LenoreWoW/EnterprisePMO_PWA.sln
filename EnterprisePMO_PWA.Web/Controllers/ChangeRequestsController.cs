using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize]
    public class ChangeRequestsController : Controller
    {
        private readonly ChangeRequestService _changeRequestService;
        public ChangeRequestsController(ChangeRequestService changeRequestService)
        {
            _changeRequestService = changeRequestService;
        }

        [Authorize(Roles = "ProjectManager,SubPMO,MainPMO")]
        public IActionResult List() => View(); // Retrieve data in the view

        [Authorize(Roles = "ProjectManager")]
        [HttpGet]
        public IActionResult Create() => View();

        [Authorize(Roles = "ProjectManager")]
        [HttpPost]
        public async Task<IActionResult> Create(ChangeRequest cr)
        {
            if (ModelState.IsValid)
            {
                await _changeRequestService.CreateChangeRequestAsync(cr);
                return RedirectToAction("List");
            }
            return View(cr);
        }

        [Authorize(Roles = "ProjectManager")]
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var cr = _changeRequestService.GetChangeRequestById(id);
            if (cr == null) return NotFound();
            return View(cr);
        }

        [Authorize(Roles = "ProjectManager")]
        [HttpPost]
        public async Task<IActionResult> Edit(ChangeRequest cr)
        {
            if (ModelState.IsValid)
            {
                await _changeRequestService.UpdateChangeRequestAsync(cr);
                return RedirectToAction("List");
            }
            return View(cr);
        }

        [Authorize(Roles = "SubPMO,MainPMO")]
        [HttpPost]
        public async Task<IActionResult> Approve(Guid id)
        {
            await _changeRequestService.ApproveChangeRequestAsync(id);
            return RedirectToAction("List");
        }

        [Authorize(Roles = "SubPMO,MainPMO")]
        [HttpPost]
        public async Task<IActionResult> Reject(Guid id, string reason)
        {
            await _changeRequestService.RejectChangeRequestAsync(id, reason);
            return RedirectToAction("List");
        }
    }
}
