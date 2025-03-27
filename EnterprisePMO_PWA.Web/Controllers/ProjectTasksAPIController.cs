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
    [Route("api/projects")]
    [ApiController]
    [Authorize]
    public class ProjectTasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public ProjectTasksController(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: api/projects/{projectId}/tasks
        [HttpGet("{projectId}/tasks")]
        public async Task<ActionResult<IEnumerable<ProjectTask>>> GetProjectTasks(Guid projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            var tasks = await _context.ProjectTasks
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.StartDate)
                .ToListAsync();

            if (tasks.Count == 0)
            {
                // For demo purposes, if no tasks exist, create sample tasks
                return Ok(GenerateSampleTasks(projectId, project.StartDate, project.EndDate));
            }

            return Ok(tasks);
        }

        // GET: api/projects/{projectId}/milestones
        [HttpGet("{projectId}/milestones")]
        public async Task<ActionResult<IEnumerable<ProjectMilestone>>> GetProjectMilestones(Guid projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            var milestones = await _context.ProjectMilestones
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.Date)
                .ToListAsync();

            if (milestones.Count == 0)
            {
                // For demo purposes, if no milestones exist, create sample milestones
                return Ok(GenerateSampleMilestones(projectId, project.StartDate, project.EndDate));
            }

            return Ok(milestones);
        }

        // POST: api/projects/{projectId}/tasks
        [HttpPost("{projectId}/tasks")]
        [Authorize(Roles = "ProjectManager,SubPMO,MainPMO")]
        public async Task<ActionResult<ProjectTask>> CreateProjectTask(Guid projectId, ProjectTask task)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            task.Id = Guid.NewGuid();
            task.ProjectId = projectId;
            task.CreatedDate = DateTime.UtcNow;

            // Validate dates are within project timeframe
            if (task.StartDate < project.StartDate || task.EndDate > project.EndDate)
            {
                return BadRequest("Task dates must be within the project timeframe");
            }

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();

            // Log the action
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                await _auditService.LogActionAsync(
                    "ProjectTask",
                    task.Id,
                    "Create",
                    $"Task '{task.Name}' created for project {projectId}"
                );
            }

            return CreatedAtAction(nameof(GetProjectTasks), new { projectId = projectId }, task);
        }

        // PUT: api/projects/{projectId}/tasks/{id}
        [HttpPut("{projectId}/tasks/{id}")]
        [Authorize(Roles = "ProjectManager,SubPMO,MainPMO")]
        public async Task<IActionResult> UpdateProjectTask(Guid projectId, Guid id, ProjectTask task)
        {
            if (id != task.Id || projectId != task.ProjectId)
            {
                return BadRequest("ID mismatch");
            }

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            // Retrieve original task for comparison
            var originalTask = await _context.ProjectTasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (originalTask == null)
            {
                return NotFound("Task not found");
            }

            // Validate dates are within project timeframe
            if (task.StartDate < project.StartDate || task.EndDate > project.EndDate)
            {
                return BadRequest("Task dates must be within the project timeframe");
            }

            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Log the action with changes
                var userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    await _auditService.LogActionAsync(
                        "ProjectTask",
                        task.Id,
                        "Update",
                        _auditService.CreateChangeSummary(originalTask, task)
                    );
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/projects/{projectId}/tasks/{id}
        [HttpDelete("{projectId}/tasks/{id}")]
        [Authorize(Roles = "ProjectManager,SubPMO,MainPMO")]
        public async Task<IActionResult> DeleteProjectTask(Guid projectId, Guid id)
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null || task.ProjectId != projectId)
            {
                return NotFound();
            }

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();

            // Log the action
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                await _auditService.LogActionAsync(
                    "ProjectTask",
                    id,
                    "Delete",
                    $"Task '{task.Name}' deleted from project {projectId}"
                );
            }

            return NoContent();
        }

        // POST: api/projects/{projectId}/milestones
        [HttpPost("{projectId}/milestones")]
        [Authorize(Roles = "ProjectManager,SubPMO,MainPMO")]
        public async Task<ActionResult<ProjectMilestone>> CreateProjectMilestone(Guid projectId, ProjectMilestone milestone)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            milestone.Id = Guid.NewGuid();
            milestone.ProjectId = projectId;
            milestone.CreatedDate = DateTime.UtcNow;

            // Validate date is within project timeframe
            if (milestone.Date < project.StartDate || milestone.Date > project.EndDate)
            {
                return BadRequest("Milestone date must be within the project timeframe");
            }

            _context.ProjectMilestones.Add(milestone);
            await _context.SaveChangesAsync();

            // Log the action
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                await _auditService.LogActionAsync(
                    "ProjectMilestone",
                    milestone.Id,
                    "Create",
                    $"Milestone '{milestone.Name}' created for project {projectId}"
                );
            }

            return CreatedAtAction(nameof(GetProjectMilestones), new { projectId = projectId }, milestone);
        }

        // Helper method to check if a task exists
        private bool TaskExists(Guid id)
        {
            return _context.ProjectTasks.Any(e => e.Id == id);
        }

        // Helper method to get the current user ID from claims
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            return null;
        }

        // Helper method to generate sample tasks for demo purposes
        private List<ProjectTask> GenerateSampleTasks(Guid projectId, DateTime projectStart, DateTime projectEnd)
        {
            var totalDays = (projectEnd - projectStart).TotalDays;
            var phaseDuration = totalDays / 5; // Split project into 5 phases

            var tasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Requirements Gathering",
                    Description = "Define project scope and gather all requirements",
                    StartDate = projectStart,
                    EndDate = projectStart.AddDays(phaseDuration),
                    Progress = 100, // Completed
                    Status = "Completed",
                    Priority = TaskPriority.High,
                    CreatedDate = DateTime.UtcNow.AddDays(-30)
                },
                new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Design Phase",
                    Description = "Create technical design and architecture",
                    StartDate = projectStart.AddDays(phaseDuration),
                    EndDate = projectStart.AddDays(phaseDuration * 2),
                    Progress = 80, // Almost complete
                    Status = "In Progress",
                    Priority = TaskPriority.High,
                    CreatedDate = DateTime.UtcNow.AddDays(-25)
                },
                new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Development",
                    Description = "Code implementation based on design",
                    StartDate = projectStart.AddDays(phaseDuration * 1.5), // Overlap with design
                    EndDate = projectStart.AddDays(phaseDuration * 3.5),
                    Progress = 50, // Halfway
                    Status = "In Progress",
                    Priority = TaskPriority.Medium,
                    CreatedDate = DateTime.UtcNow.AddDays(-20)
                },
                new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Testing",
                    Description = "QA and user acceptance testing",
                    StartDate = projectStart.AddDays(phaseDuration * 3),
                    EndDate = projectStart.AddDays(phaseDuration * 4),
                    Progress = 20, // Just started
                    Status = "In Progress",
                    Priority = TaskPriority.High,
                    CreatedDate = DateTime.UtcNow.AddDays(-15)
                },
                new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Deployment",
                    Description = "Release to production",
                    StartDate = projectStart.AddDays(phaseDuration * 4),
                    EndDate = projectEnd,
                    Progress = 0, // Not started
                    Status = "Not Started",
                    Priority = TaskPriority.Critical,
                    CreatedDate = DateTime.UtcNow.AddDays(-10)
                }
            };

            return tasks;
        }

        // Helper method to generate sample milestones for demo purposes
        private List<ProjectMilestone> GenerateSampleMilestones(Guid projectId, DateTime projectStart, DateTime projectEnd)
        {
            var totalDays = (projectEnd - projectStart).TotalDays;
            
            var milestones = new List<ProjectMilestone>
            {
                new ProjectMilestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Project Kickoff",
                    Description = "Official start of the project",
                    Date = projectStart,
                    Status = MilestoneStatus.Completed,
                    CreatedDate = DateTime.UtcNow.AddDays(-30)
                },
                new ProjectMilestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Design Approval",
                    Description = "Design documents approved by stakeholders",
                    Date = projectStart.AddDays(totalDays * 0.25),
                    Status = MilestoneStatus.Completed,
                    CreatedDate = DateTime.UtcNow.AddDays(-25)
                },
                new ProjectMilestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Alpha Release",
                    Description = "First testable version",
                    Date = projectStart.AddDays(totalDays * 0.5),
                    Status = MilestoneStatus.InProgress,
                    CreatedDate = DateTime.UtcNow.AddDays(-20)
                },
                new ProjectMilestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Beta Release",
                    Description = "Feature complete version for testing",
                    Date = projectStart.AddDays(totalDays * 0.75),
                    Status = MilestoneStatus.Pending,
                    CreatedDate = DateTime.UtcNow.AddDays(-15)
                },
                new ProjectMilestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    Name = "Final Release",
                    Description = "Production deployment",
                    Date = projectEnd,
                    Status = MilestoneStatus.Pending,
                    CreatedDate = DateTime.UtcNow.AddDays(-10)
                }
            };

            return milestones;
        }
    }
}