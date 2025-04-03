using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using TaskStatus = EnterprisePMO_PWA.Domain.Enums.TaskStatus;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Provides methods for managing weekly project updates.
    /// </summary>
    public class WeeklyUpdateService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public WeeklyUpdateService(
            AppDbContext context,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        /// <summary>
        /// Retrieves a weekly update by ID.
        /// </summary>
        public WeeklyUpdate? GetWeeklyUpdateById(Guid id)
        {
            return _context.WeeklyUpdates
                .Include(w => w.Project)
                .Include(w => w.SubmittedByUser)
                .FirstOrDefault(w => w.Id == id);
        }

        /// <summary>
        /// Gets weekly updates for a project.
        /// </summary>
        public List<WeeklyUpdate> GetWeeklyUpdatesByProject(Guid projectId)
        {
            return _context.WeeklyUpdates
                .Where(w => w.ProjectId == projectId)
                .OrderByDescending(w => w.WeekEndingDate)
                .ToList();
        }

        /// <summary>
        /// Gets the most recent updates across all projects.
        /// </summary>
        public async Task<List<WeeklyUpdate>> GetRecentUpdates(int count)
        {
            return await _context.WeeklyUpdates
                .Include(w => w.Project)
                .Include(w => w.SubmittedByUser)
                .OrderByDescending(w => w.SubmittedDate)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Submits a new weekly update.
        /// </summary>
        public async Task<WeeklyUpdate> SubmitWeeklyUpdateAsync(WeeklyUpdate update, Guid userId)
        {
            // Verify the user has permission to submit updates
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.Role != UserRole.ProjectManager)
            {
                throw new UnauthorizedAccessException("Only project managers can submit weekly updates");
            }

            update.Id = Guid.NewGuid();
            update.SubmittedDate = DateTime.UtcNow;
            update.IsApprovedBySubPMO = false;
            update.IsSentBack = false;
            
            // Calculate status color based on percent complete and any issues
            update.StatusColor = CalculateStatusColor(update);
            
            _context.WeeklyUpdates.Add(update);
            await _context.SaveChangesAsync();
            
            // Find Sub PMO users to notify
            if (_notificationService != null)
            {
                var project = await _context.Projects
                    .Include(p => p.Department)
                    .FirstOrDefaultAsync(p => p.Id == update.ProjectId);
                    
                if (project != null && project.Department != null)
                {
                    var subPMOUsers = await _context.Users
                        .Where(u => u.Role == UserRole.SubPMO && u.DepartmentId == project.DepartmentId)
                        .ToListAsync();
                        
                    foreach (var subPMO in subPMOUsers)
                    {
                        await _notificationService.NotifyAsync(
                            $"New weekly update submitted for project '{project.Name}'",
                            subPMO.Username
                        );
                    }
                }
            }
            
            return update;
        }

        /// <summary>
        /// Updates an existing weekly update.
        /// </summary>
        public async Task UpdateWeeklyUpdateAsync(WeeklyUpdate update)
        {
            // Recalculate status color
            update.StatusColor = CalculateStatusColor(update);
            
            _context.Entry(update).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Approves a weekly update.
        /// </summary>
        public async Task ApproveWeeklyUpdateAsync(Guid id)
        {
            var update = await _context.WeeklyUpdates
                .Include(w => w.Project)
                .Include(w => w.SubmittedByUser)
                .FirstOrDefaultAsync(w => w.Id == id);
                
            if (update == null)
                throw new ArgumentException("Weekly update not found", nameof(id));
                
            update.IsApprovedBySubPMO = true;
            update.ApprovedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            // Notify the project manager
            if (_notificationService != null && update.SubmittedByUser != null)
            {
                await _notificationService.NotifyAsync(
                    $"Your weekly update for project '{update.Project?.Name}' has been approved",
                    update.SubmittedByUser.Username
                );
            }
        }

        /// <summary>
        /// Rejects a weekly update.
        /// </summary>
        public async Task RejectWeeklyUpdateAsync(Guid id)
        {
            var update = await _context.WeeklyUpdates
                .Include(w => w.Project)
                .Include(w => w.SubmittedByUser)
                .FirstOrDefaultAsync(w => w.Id == id);
                
            if (update == null)
                throw new ArgumentException("Weekly update not found", nameof(id));
                
            update.IsSentBack = true;
            
            await _context.SaveChangesAsync();
            
            // Notify the project manager
            if (_notificationService != null && update.SubmittedByUser != null)
            {
                await _notificationService.NotifyAsync(
                    $"Your weekly update for project '{update.Project?.Name}' requires revision",
                    update.SubmittedByUser.Username
                );
            }
        }

        /// <summary>
        /// Deletes a weekly update.
        /// </summary>
        public async Task DeleteWeeklyUpdateAsync(Guid id)
        {
            var update = await _context.WeeklyUpdates.FindAsync(id);
            if (update != null)
            {
                _context.WeeklyUpdates.Remove(update);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Calculates the status color based on percent complete and issues.
        /// </summary>
        private StatusColor CalculateStatusColor(WeeklyUpdate update)
        {
            // If there are critical issues/risks, set status to Red
            if (!string.IsNullOrEmpty(update.IssuesOrRisks) && 
                (update.IssuesOrRisks.Contains("critical", StringComparison.OrdinalIgnoreCase) ||
                 update.IssuesOrRisks.Contains("blocker", StringComparison.OrdinalIgnoreCase) ||
                 update.IssuesOrRisks.Contains("severe", StringComparison.OrdinalIgnoreCase)))
            {
                return StatusColor.Red;
            }
            
            // If percent complete is high, set to Green
            if (update.PercentComplete >= 90)
            {
                return StatusColor.Green;
            }
            
            // If ahead of schedule, set to Blue
            if (update.PercentComplete > 80 && !string.IsNullOrEmpty(update.Accomplishments) &&
                update.Accomplishments.Contains("ahead", StringComparison.OrdinalIgnoreCase))
            {
                return StatusColor.Blue;
            }
            
            // If percent complete is moderate, set to Yellow
            if (update.PercentComplete >= 50)
            {
                return StatusColor.Yellow;
            }
            
            // If percent complete is low, set to Red
            return StatusColor.Red;
        }

        public async Task SendWeeklyUpdatesAsync()
        {
            var projects = await _context.Projects
                .Include(p => p.ProjectManager)
                .Include(p => p.Tasks)
                .Where(p => p.Status == ProjectStatus.Active)
                .ToListAsync();

            var updates = new List<WeeklyUpdate>();

            foreach (var project in projects)
            {
                var update = new WeeklyUpdate
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    WeekNumber = GetISOWeekNumber(DateTime.UtcNow),
                    Year = DateTime.UtcNow.Year,
                    CompletedTasks = project.Tasks.Count(t => t.Status == TaskStatus.Completed),
                    TotalTasks = project.Tasks.Count,
                    Status = project.Status,
                    Progress = CalculateProgress(project)
                };

                updates.Add(update);
            }

            await _context.WeeklyUpdates.AddRangeAsync(updates);
            await _context.SaveChangesAsync();

            // Notify PMO Head and Project Managers
            var stakeholders = await _context.Users
                .Where(u => u.Role == UserRole.PMOHead || u.Role == UserRole.ProjectManager)
                .ToListAsync();

            foreach (var stakeholder in stakeholders)
            {
                var relevantUpdates = stakeholder.Role == UserRole.PMOHead
                    ? updates // PMO Head sees all updates
                    : updates.Where(u => projects.Any(p => p.ProjectManagerId == stakeholder.Id && p.Id == u.ProjectId));

                if (relevantUpdates.Any())
                {
                    await SendWeeklyUpdateEmailAsync(stakeholder, relevantUpdates);
                }
            }
        }

        private async Task SendWeeklyUpdateEmailAsync(User recipient, IEnumerable<WeeklyUpdate> updates)
        {
            var emailBody = GenerateWeeklyUpdateEmailBody(updates);
            var weekNumber = GetISOWeekNumber(DateTime.UtcNow);

            await _emailService.SendEmailAsync(
                recipient.Email,
                $"Weekly Project Updates - Week {weekNumber}",
                emailBody
            );

            await _notificationService.NotifyAsync(
                $"Weekly project updates for Week {weekNumber} are available",
                recipient.Username
            );
        }

        private string GenerateWeeklyUpdateEmailBody(IEnumerable<WeeklyUpdate> updates)
        {
            var body = "Weekly Project Updates\n\n";

            foreach (var update in updates)
            {
                body += $"Project: {update.ProjectName}\n";
                body += $"Status: {update.Status}\n";
                body += $"Progress: {update.Progress:P0}\n";
                body += $"Tasks Completed: {update.CompletedTasks}/{update.TotalTasks}\n\n";
            }

            return body;
        }

        private double CalculateProgress(Project project)
        {
            if (!project.Tasks.Any())
                return 0;

            return (double)project.Tasks.Count(t => t.Status == TaskStatus.Completed) / project.Tasks.Count;
        }

        private int GetISOWeekNumber(DateTime date)
        {
            return System.Globalization.ISOWeek.GetWeekOfYear(date);
        }
    }
}