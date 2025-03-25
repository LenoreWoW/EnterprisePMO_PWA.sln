using System;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
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

        public ChangeRequestService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new change request.
        /// </summary>
        public async Task<ChangeRequest> CreateChangeRequestAsync(ChangeRequest cr)
        {
            cr.Id = Guid.NewGuid();
            cr.RequestDate = DateTime.UtcNow;
            cr.ApprovalStatus = ChangeRequestStatus.Proposed;
            _context.ChangeRequests.Add(cr);
            await _context.SaveChangesAsync();
            return cr;
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
        /// Approves a change request.
        /// </summary>
        public async Task ApproveChangeRequestAsync(Guid id)
        {
            var cr = await _context.ChangeRequests.FirstOrDefaultAsync(c => c.Id == id);
            if (cr != null)
            {
                cr.ApprovalStatus = ChangeRequestStatus.MainPMOApproved;
                cr.MainPMOApprovalDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Rejects a change request.
        /// </summary>
        public async Task RejectChangeRequestAsync(Guid id)
        {
            var cr = await _context.ChangeRequests.FirstOrDefaultAsync(c => c.Id == id);
            if (cr != null)
            {
                cr.ApprovalStatus = ChangeRequestStatus.Rejected;
                await _context.SaveChangesAsync();
            }
        }
    }
}
