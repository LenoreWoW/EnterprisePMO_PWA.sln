using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Services;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
        /// Submits a project for approval by the Sub PMO.
        /// </summary>
        public async Task<bool> SubmitForApprovalAsync(Guid projectId, Guid submittedByUserId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the project is in a state that can be submitted
            if (project.Status != ProjectStatus.Proposed)
            {
                throw new InvalidOperationException("Only proposed projects can be submitted for approval");
            }
            
            // Create audit log entry
            await _auditService.LogActionAsync(
                "Project",
                project.Id,
                "SubmitForApproval",
                $"Project '{project.Name}' submitted for approval by {submittedByUserId}"
            );
            
            // Get Sub PMO users to notify
            var subPMOUsers = await _context.Users
                .Where(u => u.Role == RoleType.SubPMO)
                .ToListAsync();
                
            // Send notification to Sub PMO users
            foreach (var subPMO in subPMOUsers)
            {
                await _notificationService.NotifyAsync(
                    $"Project '{project.Name}' has been submitted for your approval", 
                    subPMO.Username
                );
                
                // Queue email notification
                _notificationService.EnqueueEmailNotification(
                    subPMO.Username,
                    $"New Project Approval Request: {project.Name}",
                    $"Dear Sub PMO,\n\nA new project '{project.Name}' has been submitted for your approval by {project.ProjectManager?.Username ?? "a project manager"}.\n\nPlease review and take action.\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            return true;
        }
        
        /// <summary>
        /// Approves a project by the Sub PMO and moves it to the Main PMO approval queue.
        /// </summary>
        public async Task<bool> ApproveBySubPMOAsync(Guid projectId, Guid approvedByUserId, string comments)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the user has Sub PMO permissions
            if (!await _permissionService.HasPermissionAsync(approvedByUserId, "ApproveRequests"))
            {
                throw new UnauthorizedAccessException("User does not have Sub PMO approval permissions");
            }
            
            // Create audit log entry
            await _auditService.LogActionAsync(
                "Project",
                project.Id,
                "SubPMOApproval",
                $"Project '{project.Name}' approved by Sub PMO user {approvedByUserId}. Comments: {comments}"
            );
            
            // Update project status
            project.Status = ProjectStatus.Active; // Intermediate state before Main PMO approval
            project.StatusColor = project.ComputeStatusColor();
            await _context.SaveChangesAsync();
            
            // Get Main PMO users to notify
            var mainPMOUsers = await _context.Users
                .Where(u => u.Role == RoleType.MainPMO)
                .ToListAsync();
                
            // Send notification to Main PMO users
            foreach (var mainPMO in mainPMOUsers)
            {
                await _notificationService.NotifyAsync(
                    $"Project '{project.Name}' has been approved by Sub PMO and awaits your review", 
                    mainPMO.Username
                );
                
                // Queue email notification
                _notificationService.EnqueueEmailNotification(
                    mainPMO.Username,
                    $"Project Approved by Sub PMO: {project.Name}",
                    $"Dear Main PMO,\n\nProject '{project.Name}' has been approved by Sub PMO and requires your final approval.\n\nSub PMO Comments: {comments}\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            // Notify the project manager
            if (project.ProjectManager != null)
            {
                await _notificationService.NotifyAsync(
                    $"Your project '{project.Name}' has been approved by Sub PMO and is awaiting Main PMO review",
                    project.ProjectManager.Username
                );
                
                _notificationService.EnqueueEmailNotification(
                    project.ProjectManager.Username,
                    $"Project Update: {project.Name} - Sub PMO Approved",
                    $"Dear Project Manager,\n\nYour project '{project.Name}' has been approved by Sub PMO and is now awaiting Main PMO review.\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            return true;
        }
        
        /// <summary>
        /// Rejects a project by the Sub PMO and returns it to the project manager.
        /// </summary>
        public async Task<bool> RejectBySubPMOAsync(Guid projectId, Guid rejectedByUserId, string rejectionReason)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the user has Sub PMO permissions
            if (!await _permissionService.HasPermissionAsync(rejectedByUserId, "ApproveRequests"))
            {
                throw new UnauthorizedAccessException("User does not have Sub PMO rejection permissions");
            }
            
            // Create audit log entry
            await _auditService.LogActionAsync(
                "Project",
                project.Id,
                "SubPMORejection",
                $"Project '{project.Name}' rejected by Sub PMO user {rejectedByUserId}. Reason: {rejectionReason}"
            );
            
            // Update project status
            project.Status = ProjectStatus.Rejected;
            project.StatusColor = StatusColor.Red;
            await _context.SaveChangesAsync();
            
            // Notify the project manager
            if (project.ProjectManager != null)
            {
                await _notificationService.NotifyAsync(
                    $"Your project '{project.Name}' has been rejected by Sub PMO: {rejectionReason}",
                    project.ProjectManager.Username
                );
                
                _notificationService.EnqueueEmailNotification(
                    project.ProjectManager.Username,
                    $"Project Rejected: {project.Name}",
                    $"Dear Project Manager,\n\nYour project '{project.Name}' has been rejected by Sub PMO.\n\nReason: {rejectionReason}\n\nPlease make necessary adjustments and resubmit.\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            return true;
        }
        
        /// <summary>
        /// Final approval by the Main PMO.
        /// </summary>
        public async Task<bool> ApproveByMainPMOAsync(Guid projectId, Guid approvedByUserId, string comments)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the user has Main PMO permissions
            var user = await _context.Users.FindAsync(approvedByUserId);
            if (user == null || user.Role != RoleType.MainPMO)
            {
                throw new UnauthorizedAccessException("User does not have Main PMO approval permissions");
            }
            
            // Create audit log entry
            await _auditService.LogActionAsync(
                "Project",
                project.Id,
                "MainPMOApproval",
                $"Project '{project.Name}' approved by Main PMO user {approvedByUserId}. Comments: {comments}"
            );
            
            // Update project status and approval date
            project.Status = ProjectStatus.Active;
            project.ApprovedDate = DateTime.UtcNow;
            project.StatusColor = project.ComputeStatusColor();
            await _context.SaveChangesAsync();
            
            // Notify the project manager
            if (project.ProjectManager != null)
            {
                await _notificationService.NotifyAsync(
                    $"Your project '{project.Name}' has been officially approved by Main PMO",
                    project.ProjectManager.Username
                );
                
                _notificationService.EnqueueEmailNotification(
                    project.ProjectManager.Username,
                    $"Project Approved: {project.Name}",
                    $"Dear Project Manager,\n\nCongratulations! Your project '{project.Name}' has been officially approved by Main PMO.\n\nYou can now proceed with project execution.\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            // Notify department director if available
            if (project.Department != null)
            {
                var departmentDirector = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == RoleType.DepartmentDirector && u.DepartmentId == project.DepartmentId);
                    
                if (departmentDirector != null)
                {
                    await _notificationService.NotifyAsync(
                        $"Project '{project.Name}' in your department has been approved",
                        departmentDirector.Username
                    );
                    
                    _notificationService.EnqueueEmailNotification(
                        departmentDirector.Username,
                        $"Department Project Approved: {project.Name}",
                        $"Dear Department Director,\n\nA project in your department '{project.Name}' has been approved and is now active.\n\nRegards,\nEnterprise PMO System"
                    );
                }
            }
            
            // Notify executives
            var executives = await _context.Users
                .Where(u => u.Role == RoleType.Executive)
                .ToListAsync();
                
            foreach (var executive in executives)
            {
                _notificationService.EnqueueEmailNotification(
                    executive.Username,
                    $"New Project Approved: {project.Name}",
                    $"Dear Executive,\n\nA new project '{project.Name}' has been approved and is now active.\n\nDepartment: {project.Department?.Name ?? "N/A"}\nBudget: ${project.Budget}\nEstimated Completion: {project.EndDate.ToShortDateString()}\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            return true;
        }
        
        /// <summary>
        /// Rejection by the Main PMO.
        /// </summary>
        public async Task<bool> RejectByMainPMOAsync(Guid projectId, Guid rejectedByUserId, string rejectionReason)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the user has Main PMO permissions
            var user = await _context.Users.FindAsync(rejectedByUserId);
            if (user == null || user.Role != RoleType.MainPMO)
            {
                throw new UnauthorizedAccessException("User does not have Main PMO rejection permissions");
            }
            
            // Create audit log entry
            await _auditService.LogActionAsync(
                "Project",
                project.Id,
                "MainPMORejection",
                $"Project '{project.Name}' rejected by Main PMO user {rejectedByUserId}. Reason: {rejectionReason}"
            );
            
            // Update project status
            project.Status = ProjectStatus.Rejected;
            project.StatusColor = StatusColor.Red;
            await _context.SaveChangesAsync();
            
            // Notify the project manager
            if (project.ProjectManager != null)
            {
                await _notificationService.NotifyAsync(
                    $"Your project '{project.Name}' has been rejected by Main PMO: {rejectionReason}",
                    project.ProjectManager.Username
                );
                
                _notificationService.EnqueueEmailNotification(
                    project.ProjectManager.Username,
                    $"Project Rejected by Main PMO: {project.Name}",
                    $"Dear Project Manager,\n\nYour project '{project.Name}' has been rejected by Main PMO.\n\nReason: {rejectionReason}\n\nPlease make necessary adjustments and resubmit for approval.\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            // Notify Sub PMO users
            var subPMOUsers = await _context.Users
                .Where(u => u.Role == RoleType.SubPMO)
                .ToListAsync();
                
            foreach (var subPMO in subPMOUsers)
            {
                await _notificationService.NotifyAsync(
                    $"Project '{project.Name}' has been rejected by Main PMO",
                    subPMO.Username
                );
            }
            
            return true;
        }
        
        /// <summary>
        /// Completes a project, marking it as successfully finished.
        /// </summary>
        public async Task<bool> CompleteProjectAsync(Guid projectId, Guid completedByUserId, string completionNotes)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the project is active
            if (project.Status != ProjectStatus.Active)
            {
                throw new InvalidOperationException("Only active projects can be completed");
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
            
            // Notify project stakeholders
            var stakeholdersToNotify = new List<User>();
            
            // Add project manager
            if (project.ProjectManager != null)
            {
                stakeholdersToNotify.Add(project.ProjectManager);
            }
            
            // Add department director if available
            if (project.Department != null)
            {
                var departmentDirector = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == RoleType.DepartmentDirector && u.DepartmentId == project.DepartmentId);
                    
                if (departmentDirector != null)
                {
                    stakeholdersToNotify.Add(departmentDirector);
                }
            }
            
            // Add PMO users
            var pmoUsers = await _context.Users
                .Where(u => u.Role == RoleType.SubPMO || u.Role == RoleType.MainPMO)
                .ToListAsync();
                
            stakeholdersToNotify.AddRange(pmoUsers);
            
            // Notify all stakeholders
            foreach (var stakeholder in stakeholdersToNotify)
            {
                await _notificationService.NotifyAsync(
                    $"Project '{project.Name}' has been completed successfully",
                    stakeholder.Username
                );
                
                _notificationService.EnqueueEmailNotification(
                    stakeholder.Username,
                    $"Project Completed: {project.Name}",
                    $"Dear {stakeholder.Role},\n\nProject '{project.Name}' has been successfully completed.\n\nCompletion Notes: {completionNotes}\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            return true;
        }
        
        /// <summary>
        /// Resubmits a rejected project for approval with modifications.
        /// </summary>
        public async Task<bool> ResubmitProjectAsync(Guid projectId, Guid resubmittedByUserId, string changesDescription)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.Id == projectId);
                
            if (project == null)
                return false;
                
            // Verify the project is rejected
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
            
            // Update project status back to proposed
            project.Status = ProjectStatus.Proposed;
            project.StatusColor = project.ComputeStatusColor();
            await _context.SaveChangesAsync();
            
            // Get Sub PMO users to notify
            var subPMOUsers = await _context.Users
                .Where(u => u.Role == RoleType.SubPMO)
                .ToListAsync();
                
            // Send notification to Sub PMO users
            foreach (var subPMO in subPMOUsers)
            {
                await _notificationService.NotifyAsync(
                    $"Project '{project.Name}' has been resubmitted after addressing previous concerns", 
                    subPMO.Username
                );
                
                // Queue email notification
                _notificationService.EnqueueEmailNotification(
                    subPMO.Username,
                    $"Project Resubmitted: {project.Name}",
                    $"Dear Sub PMO,\n\nProject '{project.Name}' has been resubmitted after addressing previous concerns.\n\nChanges Made: {changesDescription}\n\nPlease review at your earliest convenience.\n\nRegards,\nEnterprise PMO System"
                );
            }
            
            return true;
        }
    }
}