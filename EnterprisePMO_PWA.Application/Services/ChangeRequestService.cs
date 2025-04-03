using System;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;
using EnterprisePMO_PWA.Domain.Interfaces;
using EnterprisePMO_PWA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Application.Services
{
    /// <summary>
    /// Provides methods to manage change requests.
    /// </summary>
    public class ChangeRequestService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public ChangeRequestService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Creates a new change request.
        /// </summary>
        public async Task<ChangeRequest> CreateChangeRequestAsync(ChangeRequest changeRequest)
        {
            changeRequest.Id = Guid.NewGuid();
            changeRequest.RequestDate = DateTime.UtcNow;
            changeRequest.Status = ChangeRequestStatus.Pending;

            _context.ChangeRequests.Add(changeRequest);
            await _context.SaveChangesAsync();

            // Notify project manager and PMO
            await NotifyChangeRequestCreated(changeRequest);

            return changeRequest;
        }

        /// <summary>
        /// Retrieves a change request by ID.
        /// </summary>
        public ChangeRequest? GetChangeRequestById(Guid id)
        {
            return _context.ChangeRequests.FirstOrDefault(cr => cr.Id == id);
        }

        /// <summary>
        /// Updates an existing change request.
        /// </summary>
        public async Task UpdateChangeRequestAsync(ChangeRequest cr)
        {
            _context.Entry(cr).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Approves a change request by the Main PMO.
        /// </summary>
        public async Task<bool> ApproveChangeRequestAsync(Guid changeRequestId)
        {
            var changeRequest = await _context.ChangeRequests
                .Include(cr => cr.Project)
                .FirstOrDefaultAsync(cr => cr.Id == changeRequestId);

            if (changeRequest == null)
                return false;

            changeRequest.Status = ChangeRequestStatus.Approved;
            changeRequest.MainPMOApprovalDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify project team
            await NotifyChangeRequestApproved(changeRequest);

            return true;
        }

        /// <summary>
        /// Rejects a change request.
        /// </summary>
        public async Task<bool> RejectChangeRequestAsync(Guid changeRequestId, string reason)
        {
            var changeRequest = await _context.ChangeRequests
                .Include(cr => cr.Project)
                .FirstOrDefaultAsync(cr => cr.Id == changeRequestId);

            if (changeRequest == null)
                return false;

            changeRequest.Status = ChangeRequestStatus.Rejected;

            await _context.SaveChangesAsync();

            // Notify project team
            await NotifyChangeRequestRejected(changeRequest, reason);

            return true;
        }

        private async Task NotifyChangeRequestCreated(ChangeRequest changeRequest)
        {
            var message = $"A new change request has been created for project {changeRequest.Project?.Name}";
            var projectManager = changeRequest.Project?.ProjectManager;
            if (projectManager != null)
            {
                await _notificationService.NotifyAsync(message, projectManager.Username);
            }
        }

        private async Task NotifyChangeRequestApproved(ChangeRequest changeRequest)
        {
            var message = $"The change request for project {changeRequest.Project?.Name} has been approved";
            var requestedBy = changeRequest.RequestedByUser;
            if (requestedBy != null)
            {
                await _notificationService.NotifyAsync(message, requestedBy.Username);
            }
        }

        private async Task NotifyChangeRequestRejected(ChangeRequest changeRequest, string reason)
        {
            var message = $"The change request for project {changeRequest.Project?.Name} has been rejected. Reason: {reason}";
            var requestedBy = changeRequest.RequestedByUser;
            if (requestedBy != null)
            {
                await _notificationService.NotifyAsync(message, requestedBy.Username);
            }
        }
    }
}
