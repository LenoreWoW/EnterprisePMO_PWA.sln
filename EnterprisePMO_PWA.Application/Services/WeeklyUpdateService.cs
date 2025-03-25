using System;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Provides methods to manage weekly updates.
    /// </summary>
    public class WeeklyUpdateService
    {
        private readonly AppDbContext _context;

        public WeeklyUpdateService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Submits a new weekly update.
        /// </summary>
        public async Task<WeeklyUpdate> SubmitWeeklyUpdateAsync(WeeklyUpdate update)
        {
            update.Id = Guid.NewGuid();
            update.SubmittedDate = DateTime.UtcNow;
            _context.WeeklyUpdates.Add(update);
            await _context.SaveChangesAsync();
            return update;
        }

        /// <summary>
        /// Retrieves a weekly update by ID.
        /// </summary>
        public WeeklyUpdate? GetWeeklyUpdateById(Guid id)
        {
            return _context.WeeklyUpdates.FirstOrDefault(u => u.Id == id);
        }

        /// <summary>
        /// Updates a weekly update.
        /// </summary>
        public async Task UpdateWeeklyUpdateAsync(WeeklyUpdate update)
        {
            _context.Entry(update).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Approves a weekly update.
        /// </summary>
        public async Task ApproveWeeklyUpdateAsync(Guid id)
        {
            var update = await _context.WeeklyUpdates.FirstOrDefaultAsync(u => u.Id == id);
            if (update != null)
            {
                update.IsApprovedBySubPMO = true;
                update.ApprovedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Rejects a weekly update.
        /// </summary>
        public async Task RejectWeeklyUpdateAsync(Guid id)
        {
            var update = await _context.WeeklyUpdates.FirstOrDefaultAsync(u => u.Id == id);
            if (update != null)
            {
                update.IsSentBack = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
