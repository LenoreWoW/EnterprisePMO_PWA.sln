using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Web.ViewComponents
{
    /// <summary>
    /// View component to display the project workflow status.
    /// </summary>
    public class ProjectWorkflowStatusViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public ProjectWorkflowStatusViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return Content("Project not found");
            }

            // Check if the current user is authorized to view this project
            // In a real implementation, you would check if the user is part of the project team
            // or has appropriate permissions

            return View(project);
        }
    }
}