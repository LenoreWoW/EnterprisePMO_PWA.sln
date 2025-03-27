using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Route("api/project-workflow")]
    [ApiController]
    [Authorize]
    public class ProjectWorkflowApiController : ControllerBase
    {
        private readonly ProjectWorkflowService _workflowService;
        private readonly AppDbContext _context;

        public ProjectWorkflowApiController(
            ProjectWorkflowService workflowService,
            AppDbContext context)
        {
            _workflowService = workflowService;
            _context = context;
        }

        // Submit project for approval by Sub PMO
        [HttpPost("submit")]
        [Authorize(Roles = "ProjectManager")]
        public async Task<IActionResult> SubmitForApproval([FromBody] WorkflowRequest request)
        {
            // Get the current user ID from claims
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            // Verify the user is the project manager
            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            if (project.ProjectManagerId != userId.Value)
            {
                return Forbid("Only the project manager can submit this project for approval");
            }

            try
            {
                bool result = await _workflowService.SubmitForApprovalAsync(request.ProjectId, userId.Value);
                return Ok(new { success = result, message = "Project submitted for approval successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Approve project by Sub PMO
        [HttpPost("approve-sub-pmo")]
        [Authorize(Roles = "SubPMO")]
        public async Task<IActionResult> ApproveBySubPMO([FromBody] ApprovalRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                bool result = await _workflowService.ApproveBySubPMOAsync(
                    request.ProjectId, 
                    userId.Value, 
                    request.Comments ?? "Approved by Sub PMO");
                
                return Ok(new { success = result, message = "Project approved by Sub PMO successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Reject project by Sub PMO
        [HttpPost("reject-sub-pmo")]
        [Authorize(Roles = "SubPMO")]
        public async Task<IActionResult> RejectBySubPMO([FromBody] RejectionRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return BadRequest(new { success = false, message = "Rejection reason is required" });
            }

            try
            {
                bool result = await _workflowService.RejectBySubPMOAsync(
                    request.ProjectId, 
                    userId.Value, 
                    request.RejectionReason);
                
                return Ok(new { success = result, message = "Project rejected by Sub PMO" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Approve project by Main PMO
        [HttpPost("approve-main-pmo")]
        [Authorize(Roles = "MainPMO")]
        public async Task<IActionResult> ApproveByMainPMO([FromBody] ApprovalRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            try
            {
                bool result = await _workflowService.ApproveByMainPMOAsync(
                    request.ProjectId, 
                    userId.Value, 
                    request.Comments ?? "Final approval by Main PMO");
                
                return Ok(new { success = result, message = "Project approved by Main PMO successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Reject project by Main PMO
        [HttpPost("reject-main-pmo")]
        [Authorize(Roles = "MainPMO")]
        public async Task<IActionResult> RejectByMainPMO([FromBody] RejectionRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return BadRequest(new { success = false, message = "Rejection reason is required" });
            }

            try
            {
                bool result = await _workflowService.RejectByMainPMOAsync(
                    request.ProjectId, 
                    userId.Value, 
                    request.RejectionReason);
                
                return Ok(new { success = result, message = "Project rejected by Main PMO" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Resubmit project after rejection
        [HttpPost("resubmit")]
        [Authorize(Roles = "ProjectManager")]
        public async Task<IActionResult> ResubmitProject([FromBody] ResubmissionRequest request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            // Verify the user is the project manager
            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
            {
                return NotFound("Project not found");
            }

            if (project.ProjectManagerId != userId.Value)
            {
                return Forbid("Only the project manager can resubmit this project");
            }

            if (string.IsNullOrWhiteSpace(request.ChangesDescription))
            {
                return BadRequest(new { success = false, message = "Description of changes is required" });
            }

            try
            {
                bool result = await _workflowService.ResubmitProjectAsync(
                    request.ProjectId, 
                    userId.Value, 
                    request.ChangesDescription);
                
                return Ok(new { success = result, message = "Project resubmitted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Helper method to get the current user ID from claims
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }
            return null;
        }
    }

    // Request models for the API
    public class WorkflowRequest
    {
        public Guid ProjectId { get; set; }
    }

    public class ApprovalRequest
    {
        public Guid ProjectId { get; set; }
        public string? Comments { get; set; }
    }

    public class RejectionRequest
    {
        public Guid ProjectId { get; set; }
        public string RejectionReason { get; set; } = "";
    }

    public class ResubmissionRequest
    {
        public Guid ProjectId { get; set; }
        public string ChangesDescription { get; set; } = "";
    }
}