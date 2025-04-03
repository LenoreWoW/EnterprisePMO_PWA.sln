using Microsoft.AspNetCore.Mvc;
using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;
using System.Linq;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Aggregates project metrics for reporting.
        /// </summary>
        [HttpGet("projectmetrics")]
        public IActionResult GetProjectMetrics()
        {
            var projects = _context.Projects.ToList();
            var completed = projects.Count(p => p.Status == ProjectStatus.Completed);
            var inProgress = projects.Count(p => p.Status == ProjectStatus.Active);
            var pending = projects.Count(p => p.Status == ProjectStatus.Draft);
            var result = new
            {
                statusLabels = new[] { "Completed", "In Progress", "Pending" },
                statusValues = new[] { completed, inProgress, pending },
                trendDates = new[] { "Jan", "Feb", "Mar", "Apr", "May" },
                trendValues = new[] { completed, inProgress, pending, 0, 0 }
            };
            return Ok(result);
        }
    }
}
