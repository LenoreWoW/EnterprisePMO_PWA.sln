using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Provides methods for managing weekly project updates.
    /// </summary>
    public class WeeklyUpdateService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService? _notificationService;

        public WeeklyUpdateService(AppDbContext context, INotificationService? notificationService = null)
        {
            _context = context;
            _notificationService = notificationService;
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
        public async Task<WeeklyUpdate> SubmitWeeklyUpdateAsync(WeeklyUpdate update)
        {
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
                        .Where(u => u.Role == RoleType.SubPMO && u.DepartmentId == project.DepartmentId)
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
    }
}