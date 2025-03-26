using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Route("api/test/workflow")]
    [ApiController]
    public class ProjectWorkflowTestController : ControllerBase
    {
        private readonly ProjectWorkflowService _workflowService;
        private readonly AppDbContext _context;
        private readonly EnhancedNotificationService _notificationService;

        public ProjectWorkflowTestController(
            ProjectWorkflowService workflowService,
            AppDbContext context,
            EnhancedNotificationService notificationService)
        {
            _workflowService = workflowService;
            _context = context;
            _notificationService = notificationService;
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
            try
            {
                // Find the project
                var project = await _context.Projects
                    .Include(p => p.ProjectManager)
                    .FirstOrDefaultAsync(p => p.Id == projectId)
                    ?? throw new Exception("Project not found");

                // Get users for each role
                var projectManager = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == RoleType.ProjectManager)
                    ?? throw new Exception("No Project Manager found");

                var subPmo = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == RoleType.SubPMO)
                    ?? throw new Exception("No Sub PMO user found");

                var mainPmo = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == RoleType.MainPMO)
                    ?? throw new Exception("No Main PMO user found");

                // Execute the complete workflow
                var steps = new List<object>();

                // Step 1: Submit for approval
                await _workflowService.SubmitForApprovalAsync(projectId, projectManager.Id);
                steps.Add(new { step = "Submit for approval", status = "Success" });

                // Step 2: Sub PMO approval
                await _workflowService.ApproveBySubPMOAsync(projectId, subPmo.Id, "Approved by Sub PMO in test");
                steps.Add(new { step = "Sub PMO approval", status = "Success" });

                // Step 3: Main PMO approval
                await _workflowService.ApproveByMainPMOAsync(projectId, mainPmo.Id, "Final approval by Main PMO in test");
                steps.Add(new { step = "Main PMO approval", status = "Success" });

                // Get all notifications created
                var notifications = await _context.Notifications.ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Complete workflow test executed successfully",
                    workflows = steps,
                    notifications = notifications.Select(n => new
                    {
                        n.Id,
                        n.Message,
                        n.Type,
                        Recipient = n.UserId,
                        n.IsRead,
                        Created = n.CreatedAt
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error executing test workflow",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("test-workflow-rejection/{projectId}")]
        public async Task<IActionResult> TestRejectionWorkflow(Guid projectId)
        {
            try
            {
                // Find the project
                var project = await _context.Projects
                    .Include(p => p.ProjectManager)
                    .FirstOrDefaultAsync(p => p.Id == projectId)
                    ?? throw new Exception("Project not found");

                // Get users for each role
                var projectManager = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == RoleType.ProjectManager)
                    ?? throw new Exception("No Project Manager found");

                var subPmo = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == RoleType.SubPMO)
                    ?? throw new Exception("No Sub PMO user found");

                // Execute the rejection workflow
                var steps = new List<object>();

                // Step 1: Submit for approval
                await _workflowService.SubmitForApprovalAsync(projectId, projectManager.Id);
                steps.Add(new { step = "Submit for approval", status = "Success" });

                // Step 2: Sub PMO rejection
                await _workflowService.RejectBySubPMOAsync(projectId, subPmo.Id, "Rejected by Sub PMO in test - needs more details");
                steps.Add(new { step = "Sub PMO rejection", status = "Success" });

                // Step 3: Resubmit project after rejection
                await _workflowService.ResubmitProjectAsync(projectId, projectManager.Id, "Added more details as requested");
                steps.Add(new { step = "Project resubmission", status = "Success" });

                // Get all notifications created
                var notifications = await _context.Notifications.ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "Rejection workflow test executed successfully",
                    workflows = steps,
                    notifications = notifications.Select(n => new
                    {
                        n.Id,
                        n.Message,
                        n.Type,
                        Recipient = n.UserId,
                        n.IsRead,
                        Created = n.CreatedAt
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error executing rejection workflow",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}