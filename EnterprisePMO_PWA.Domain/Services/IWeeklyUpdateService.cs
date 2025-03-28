using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;

namespace EnterprisePMO_PWA.Domain.Services
{
    /// <summary>
    /// Defines methods for managing weekly project updates.
    /// </summary>
    public interface IWeeklyUpdateService
    {
        /// <summary>
        /// Creates a new weekly update.
        /// </summary>
        Task<WeeklyUpdate> CreateWeeklyUpdateAsync(WeeklyUpdate update);

        /// <summary>
        /// Gets a weekly update by ID.
        /// </summary>
        Task<WeeklyUpdate?> GetWeeklyUpdateByIdAsync(Guid id);

        /// <summary>
        /// Gets all weekly updates for a project.
        /// </summary>
        Task<List<WeeklyUpdate>> GetWeeklyUpdatesByProjectAsync(Guid projectId);

        /// <summary>
        /// Gets the latest weekly update for a project.
        /// </summary>
        Task<WeeklyUpdate?> GetLatestWeeklyUpdateAsync(Guid projectId);

        /// <summary>
        /// Gets all weekly updates pending approval.
        /// </summary>
        Task<List<WeeklyUpdate>> GetPendingApprovalsAsync();

        /// <summary>
        /// Updates an existing weekly update.
        /// </summary>
        Task UpdateWeeklyUpdateAsync(WeeklyUpdate update);

        /// <summary>
        /// Approves a weekly update.
        /// </summary>
        Task ApproveWeeklyUpdateAsync(Guid id, Guid approvedByUserId);

        /// <summary>
        /// Rejects/returns a weekly update for revision.
        /// </summary>
        Task ReturnWeeklyUpdateAsync(Guid id, string comments);

        /// <summary>
        /// Deletes a weekly update.
        /// </summary>
        Task DeleteWeeklyUpdateAsync(Guid id);
    }
}