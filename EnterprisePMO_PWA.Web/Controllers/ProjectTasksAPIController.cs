using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Application.Services;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectTasksApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public ProjectTasksApiController(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: api/projects/{projectId}/tasks
        [HttpGet("{projectId}/tasks")]
        public async Task<ActionResult<IEnumerable<object>>> GetProjectTasks(Guid projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            // For this example, we'll generate sample tasks
            // In a real implementation, you would fetch from database
            return Ok(GenerateSampleTasks(projectId, project.StartDate, project.EndDate));
        }

        // GET: api/projects/{projectId}/milestones
        [HttpGet("{projectId}/milestones")]
        public async Task<ActionResult<IEnumerable<object>>> GetProjectMilestones(Guid projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            // For this example, we'll generate sample milestones
            // In a real implementation, you would fetch from database
            return Ok(GenerateSampleMilestones(projectId, project.StartDate, project.EndDate));
        }

        // Helper method to generate sample tasks for demo purposes
        private List<object> GenerateSampleTasks(Guid projectId, DateTime projectStart, DateTime projectEnd)
        {
            var totalDays = (projectEnd - projectStart).TotalDays;
            var phaseDuration = totalDays / 5; // Split project into 5 phases

            var tasks = new List<object>
            {
                new {
                    id = Guid.NewGuid(),
                    projectId = projectId,
                    name = "Requirements Gathering",
                    description = "Define project scope and gather all requirements",
                    startDate = projectStart,
                    endDate = projectStart.AddDays(phaseDuration),
                    progress = 100, // Completed
                    status = "Completed",
                    priority = "High",
                    createdDate = DateTime.UtcNow.AddDays(-30)
                },
                new {
                    id = Guid.NewGuid(),
                    projectId = projectId,
                    name = "Design Phase",
                    description = "Create technical design and architecture",
                    startDate = projectStart.AddDays(phaseDuration),
                    endDate = projectStart.AddDays(phaseDuration * 2),
                    progress = 80, // Almost complete
                    status = "In Progress",
                    priority = "High",
                    createdDate = DateTime.UtcNow.AddDays(-25)
                },
                new {
                    id = Guid.NewGuid(),
                    projectId = projectId,
                    name = "Development",
                    description = "Code implementation based on design",
                    startDate = projectStart.AddDays(phaseDuration * 1.5), // Overlap with design
                    endDate = projectStart.AddDays(phaseDuration * 3.5),
                    progress = 50, // Halfway
                    status = "In Progress",
                    priority = "Medium",
                    createdDate = DateTime.UtcNow.AddDays(-20)
                },
                new {
                    id = Guid.NewGuid(),
                    projectId = projectId,
                    name = "Testing",
                    description = "QA and user acceptance testing",
                    startDate = projectStart.AddDays(phaseDuration * 3),
                    endDate = projectStart.AddDays(phaseDuration * 4),
                    progress = 20, // Just started
                    status = "In Progress",
                    priority = "High",
                    createdDate = DateTime.UtcNow.AddDays(-15)
                },
                new {
                    id = Guid.NewGuid(),
                    projectId = projectId,
                    name = "Deployment",
                    description = "Release to production",
                    startDate = projectStart.AddDays(phaseDuration * 4),
                    endDate = projectEnd,
                    progress = 0, // Not started
                    status = "Not Started",
                    priority = "Critical",
                    createdDate = DateTime.UtcNow.AddDays(-10)
                }
            };

            return tasks;
        }

        // Helper method to generate sample milestones for demo purposes
        private List<object> GenerateSampleMilestones(Guid projectId, DateTime projectStart, DateTime projectEnd)
        {
            var totalDays = (projectEnd - projectStart).TotalDays;
            
            var milestones = new List<object>
            {
                new {
                    id = Guid.NewGuid(),
                    projectId = projectId,
                    name = "Project Kickoff",
                    description = "Official start of the project",
                    date = projectStart,
                    status = "Completed",
                    createdDate = DateTime.UtcNow.AddDays(-30)
                },
                new {
                    id = Guid.NewGuid(),
                    projectId = projectId,
                    name = "Design Approval",
                    description = "Design documents approved by stakeholders",
                    date = projectStart.AddDays(totalDays * 0.25),
                    status = "Completed",
                    createdDate = DateTime.UtcNow.AddDays(-25)
                },
                new {
                    id = Guid.NewGuid(),
                    projectId = projectId,
                    name = "Alpha Release",
                    description = "First testable version",
                    date = projectStart.AddDays(totalDays * 0.5),
                    status = "In Progress",
                    createdDate = DateTime.UtcNow.AddDays(-20)
                },
                new {
                    id = Guid.NewGuid(),
                    projectId = projectId,
                    name = "Beta Release",
                    description = "Feature complete version for testing",
                    date = projectStart.AddDays(totalDays * 0.75),
                    status = "Pending",
                    createdDate = DateTime.UtcNow.AddDays(-15)
                },
                new {
                    id = Guid.NewGuid(),
                    projectId = projectId,
                    name = "Final Release",
                    description = "Production deployment",
                    date = projectEnd,
                    status = "Pending",
                    createdDate = DateTime.UtcNow.AddDays(-10)
                }
            };

            return milestones;
        }
    }
}