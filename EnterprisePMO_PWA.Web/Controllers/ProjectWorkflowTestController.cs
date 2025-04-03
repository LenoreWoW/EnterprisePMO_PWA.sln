using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EnterprisePMO_PWA.Domain.Enums;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize]
    [Route("api/test/workflow")]
    [ApiController]
    public class ProjectWorkflowTestController : ControllerBase
    {
        private readonly ProjectWorkflowService _workflowService;
        private readonly AppDbContext _context;
        private readonly EnhancedNotificationService _notificationService;
        private readonly ProjectService _projectService;
        private readonly IAuthService _authService;

        public ProjectWorkflowTestController(
            ProjectWorkflowService workflowService,
            AppDbContext context,
            EnhancedNotificationService notificationService,
            ProjectService projectService,
            IAuthService authService)
        {
            _workflowService = workflowService;
            _context = context;
            _notificationService = notificationService;
            _projectService = projectService;
            _authService = authService;
        }
        
        // MVC Actions for user interface
        [HttpGet("ui")]
        public IActionResult Index()
        {
            return Ok(new { message = "Test UI would go here" });
        }

        [HttpGet("setup")]
        public async Task<IActionResult> SetupTestEnvironment()
        {
            try
            {
                // 1. Create a test project
                var projectManager = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == RoleType.ProjectManager)
                    ?? throw new Exception("No Project Manager found in the database");

                var department = await _context.Departments
                    .FirstAsync()
                    ?? throw new Exception("No Department found in the database");

                var project = new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Workflow Project",
                    Description = "A project for testing the workflow process",
                    DepartmentId = department.Id,
                    ProjectManagerId = projectManager.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(3),
                    Budget = 100000,
                    EstimatedCost = 95000,
                    ClientName = "Internal Test Team",
                    Category = "Testing",
                    Status = ProjectStatus.Proposed,
                    CreationDate = DateTime.UtcNow
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                // 2. Send a test notification
                await _notificationService.NotifyAsync(
                    "Test notification for workflow implementation",
                    projectManager.Username
                );

                // Return test data
                return Ok(new
                {
                    success = true,
                    message = "Test environment set up successfully",
                    testData = new
                    {
                        projectId = project.Id,
                        projectManagerId = projectManager.Id,
                        projectManagerUsername = projectManager.Username
                    },
                    nextSteps = new[]
                    {
                        "1. Use the ProjectWorkflowController to submit the project for approval",
                        "2. Log in as a Sub PMO user to approve/reject the project",
                        "3. Log in as a Main PMO user to finalize approval",
                        "4. Check notifications for each user"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error setting up test environment",
                    error = ex.Message
                });
            }
        }

        [HttpGet("test-workflow/{projectId}")]
        public async Task<IActionResult> TestCompleteWorkflow(Guid projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Test Sub PMO approval
            if (currentUser.Role == UserRole.SubPMO)
            {
                await _workflowService.ApproveBySubPMOAsync(project);
            }

            // Test Main PMO approval
            if (currentUser.Role == UserRole.MainPMO)
            {
                await _workflowService.ApproveByMainPMOAsync(project);
            }

            // Test Project Manager actions
            if (currentUser.Role == UserRole.ProjectManager)
            {
                await _workflowService.SubmitForApprovalAsync(project);
            }

            return Ok(new { message = "Workflow test completed successfully" });
        }

        [HttpGet("test-workflow-rejection/{projectId}")]
        public async Task<IActionResult> TestRejectionWorkflow(Guid projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Test Sub PMO rejection
            if (currentUser.Role == UserRole.SubPMO)
            {
                await _workflowService.RejectBySubPMOAsync(project);
            }

            // Test Main PMO rejection
            if (currentUser.Role == UserRole.MainPMO)
            {
                await _workflowService.RejectByMainPMOAsync(project);
            }

            return Ok(new { message = "Rejection workflow test completed successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestProject()
        {
            var project = new Project
            {
                Name = "Test Project",
                Description = "A test project for workflow testing",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(3),
                Status = ProjectStatus.Draft,
                Budget = 100000,
                ProjectManagerId = (await _authService.GetCurrentUserAsync())?.Id ?? Guid.Empty
            };

            await _projectService.CreateAsync(project);
            return Ok(new { projectId = project.Id });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitForApproval(Guid projectId)
        {
            var project = await _projectService.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            await _workflowService.SubmitForApprovalAsync(project);
            await _projectService.UpdateAsync(project);

            return Ok(new { message = "Project submitted for approval" });
        }

        [HttpPost]
        public async Task<IActionResult> ApproveProject(Guid projectId)
        {
            var project = await _projectService.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.Role == UserRole.SubPMO)
            {
                await _workflowService.ApproveBySubPMOAsync(project);
            }
            else if (currentUser.Role == UserRole.MainPMO)
            {
                await _workflowService.ApproveByMainPMOAsync(project);
            }

            await _projectService.UpdateAsync(project);
            return Ok(new { message = "Project approved" });
        }

        [HttpPost]
        public async Task<IActionResult> RejectProject(Guid projectId)
        {
            var project = await _projectService.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (currentUser.Role == UserRole.SubPMO)
            {
                await _workflowService.RejectBySubPMOAsync(project);
            }
            else if (currentUser.Role == UserRole.MainPMO)
            {
                await _workflowService.RejectByMainPMOAsync(project);
            }

            await _projectService.UpdateAsync(project);
            return Ok(new { message = "Project rejected" });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteProject(Guid projectId)
        {
            var project = await _projectService.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            project.Status = ProjectStatus.Completed;
            project.CompletionDate = DateTime.Now;
            await _projectService.UpdateAsync(project);

            return Ok(new { message = "Project completed" });
        }
    }
}