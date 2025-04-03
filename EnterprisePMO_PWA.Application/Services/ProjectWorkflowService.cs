using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Services;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Domain.Enums;
using EnterprisePMO_PWA.Domain.Workflows;
using Microsoft.Extensions.Logging;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Implements project workflow management including state transitions and approvals.
    /// </summary>
    public class ProjectWorkflowService
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;

        public ProjectWorkflowService(
            AppDbContext context,
            AuditService auditService,
            INotificationService notificationService,
            IPermissionService permissionService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
            _permissionService = permissionService;
        }

        /// <summary>
        /// Submits a project for approval by the PMO Head.
        /// </summary>
        public async Task<bool> SubmitForApprovalAsync(Guid projectId, Guid submittedByUserId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the project is in a state that can be submitted
            if (project.Status != ProjectStatus.Draft)
            {
                throw new InvalidOperationException("Only draft projects can be submitted for approval");
            }
            
            // Create audit log entry
            await _auditService.LogActionAsync(
                "Project",
                project.Id,
                "SubmitForApproval",
                $"Project '{project.Name}' submitted for approval by {submittedByUserId}"
            );
            
            // Get PMO Head users to notify
            var pmoHeadUsers = await _context.Users
                .Where(u => u.Role == UserRole.PMOHead)
                .ToListAsync();
                
            // Send notification to PMO Head users
            foreach (var pmoHead in pmoHeadUsers)
            {
                await _notificationService.NotifyAsync(
                    $"Project '{project.Name}' has been submitted for your approval", 
                    pmoHead.Username
                );
                
                // Queue email notification
                _notificationService.EnqueueEmailNotification(
                    pmoHead.Username,
                    $"New Project Approval Request: {project.Name}",
                    $"Dear PMO Head,\n\nA new project '{project.Name}' has been submitted for your approval by {project.ProjectManager?.Username ?? "a project manager"}.\n\nPlease review and take action.\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            return true;
        }
        
        /// <summary>
        /// Approves a project by the PMO Head.
        /// </summary>
        public async Task<bool> ApproveProjectAsync(Guid projectId, Guid approvedByUserId, string comments)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the user has PMO Head permissions
            if (!await _permissionService.HasPermissionAsync(approvedByUserId, "ApproveRequests"))
            {
                throw new UnauthorizedAccessException("User does not have PMO Head approval permissions");
            }
            
            // Create audit log entry
            await _auditService.LogActionAsync(
                "Project",
                project.Id,
                "PMOHeadApproval",
                $"Project '{project.Name}' approved by PMO Head user {approvedByUserId}. Comments: {comments}"
            );
            
            // Update project status
            project.Status = ProjectStatus.Active;
            project.StatusColor = project.ComputeStatusColor();
            await _context.SaveChangesAsync();
            
            // Notify the project manager
            if (project.ProjectManager != null)
            {
                await _notificationService.NotifyAsync(
                    $"Your project '{project.Name}' has been approved by PMO Head",
                    project.ProjectManager.Username
                );
                
                _notificationService.EnqueueEmailNotification(
                    project.ProjectManager.Username,
                    $"Project Approved: {project.Name}",
                    $"Dear Project Manager,\n\nYour project '{project.Name}' has been approved by PMO Head.\n\nComments: {comments}\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            return true;
        }
        
        /// <summary>
        /// Rejects a project by the PMO Head.
        /// </summary>
        public async Task<bool> RejectProjectAsync(Guid projectId, Guid rejectedByUserId, string rejectionReason)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the user has PMO Head permissions
            if (!await _permissionService.HasPermissionAsync(rejectedByUserId, "ApproveRequests"))
            {
                throw new UnauthorizedAccessException("User does not have PMO Head rejection permissions");
            }
            
            // Create audit log entry
            await _auditService.LogActionAsync(
                "Project",
                project.Id,
                "PMOHeadRejection",
                $"Project '{project.Name}' rejected by PMO Head user {rejectedByUserId}. Reason: {rejectionReason}"
            );
            
            // Update project status
            project.Status = ProjectStatus.Rejected;
            project.StatusColor = StatusColor.Red;
            await _context.SaveChangesAsync();
            
            // Notify the project manager
            if (project.ProjectManager != null)
            {
                await _notificationService.NotifyAsync(
                    $"Your project '{project.Name}' has been rejected by PMO Head: {rejectionReason}",
                    project.ProjectManager.Username
                );
                
                _notificationService.EnqueueEmailNotification(
                    project.ProjectManager.Username,
                    $"Project Rejected: {project.Name}",
                    $"Dear Project Manager,\n\nYour project '{project.Name}' has been rejected by PMO Head.\n\nReason: {rejectionReason}\n\nPlease make necessary adjustments and resubmit.\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            return true;
        }
        
        /// <summary>
        /// Completes a project.
        /// </summary>
        public async Task<bool> CompleteProjectAsync(Guid projectId, Guid completedByUserId, string completionNotes)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the project is in a state that can be completed
            if (project.Status != ProjectStatus.Active)
            {
                throw new InvalidOperationException("Only active projects can be marked as completed");
            }
            
            // Create audit log entry
            await _auditService.LogActionAsync(
                "Project",
                project.Id,
                "ProjectCompletion",
                $"Project '{project.Name}' marked as completed by {completedByUserId}. Notes: {completionNotes}"
            );
            
            // Update project status
            project.Status = ProjectStatus.Completed;
            project.StatusColor = StatusColor.Green;
            await _context.SaveChangesAsync();
            
            // Notify stakeholders
            var stakeholders = await _context.Users
                .Where(u => u.Role == UserRole.PMOHead || u.Role == UserRole.ProjectManager)
                .ToListAsync();
                
            foreach (var stakeholder in stakeholders)
            {
                await _notificationService.NotifyAsync(
                    $"Project '{project.Name}' has been marked as completed",
                    stakeholder.Username
                );
                
                _notificationService.EnqueueEmailNotification(
                    stakeholder.Username,
                    $"Project Completed: {project.Name}",
                    $"Dear Stakeholder,\n\nProject '{project.Name}' has been marked as completed.\n\nCompletion Notes: {completionNotes}\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            return true;
        }
        
        /// <summary>
        /// Resubmits a rejected project for approval.
        /// </summary>
        public async Task<bool> ResubmitProjectAsync(Guid projectId, Guid resubmittedByUserId, string changesDescription)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the project is in a state that can be resubmitted
            if (project.Status != ProjectStatus.Rejected)
            {
                throw new InvalidOperationException("Only rejected projects can be resubmitted");
            }
            
            // Create audit log entry
            await _auditService.LogActionAsync(
                "Project",
                project.Id,
                "ProjectResubmission",
                $"Project '{project.Name}' resubmitted by {resubmittedByUserId}. Changes: {changesDescription}"
            );
            
            // Update project status
            project.Status = ProjectStatus.Draft;
            project.StatusColor = StatusColor.Yellow;
            await _context.SaveChangesAsync();
            
            // Get PMO Head users to notify
            var pmoHeadUsers = await _context.Users
                .Where(u => u.Role == UserRole.PMOHead)
                .ToListAsync();
                
            // Send notification to PMO Head users
            foreach (var pmoHead in pmoHeadUsers)
            {
                await _notificationService.NotifyAsync(
                    $"Project '{project.Name}' has been resubmitted for approval", 
                    pmoHead.Username
                );
                
                _notificationService.EnqueueEmailNotification(
                    pmoHead.Username,
                    $"Project Resubmitted: {project.Name}",
                    $"Dear PMO Head,\n\nProject '{project.Name}' has been resubmitted for approval.\n\nChanges Made: {changesDescription}\n\nPlease review and take action.\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            return true;
        }
    }
}