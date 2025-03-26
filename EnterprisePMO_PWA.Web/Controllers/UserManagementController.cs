using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Application.Services;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize(Roles = "Admin,MainPMO")]
    public class UserManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;
        private readonly INotificationService _notificationService;

        public UserManagementController(
            AppDbContext context,
            AuditService auditService,
            INotificationService notificationService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        // GET: /UserManagement
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .OrderBy(u => u.Username)
                .ToListAsync();

            return View(users);
        }

        // GET: /UserManagement/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: /UserManagement/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Get departments for dropdown
            ViewBag.Departments = await _context.Departments.ToListAsync();
            
            // Get roles for dropdown
            ViewBag.Roles = Enum.GetValues(typeof(RoleType))
                .Cast<RoleType>()
                .Select(r => new { Id = (int)r, Name = r.ToString() })
                .ToList();

            return View(user);
        }

        // POST: /UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current user data to track changes
                    var currentUser = await _context.Users
                        .AsNoTracking()
                        .Include(u => u.Department)
                        .FirstOrDefaultAsync(u => u.Id == id);

                    if (currentUser == null)
                    {
                        return NotFound();
                    }

                    bool departmentChanged = currentUser.DepartmentId != user.DepartmentId;
                    bool roleChanged = currentUser.Role != user.Role;

                    // Update the user
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    // Log the changes in audit trail
                    string changeSummary = "User details updated";
                    if (departmentChanged)
                    {
                        var newDepartment = await _context.Departments.FindAsync(user.DepartmentId);
                        changeSummary += $". Department changed to {newDepartment?.Name ?? "None"}";
                    }
                    if (roleChanged)
                    {
                        changeSummary += $". Role changed to {user.Role}";
                    }

                    await _auditService.LogActionAsync(
                        "User",
                        user.Id,
                        "Update",
                        changeSummary);

                    // Send notification to user
                    if (departmentChanged || roleChanged)
                    {
                        string notificationMsg = "Your account has been updated";
                        if (departmentChanged)
                        {
                            var newDepartment = await _context.Departments.FindAsync(user.DepartmentId);
                            notificationMsg += $". You have been assigned to the {newDepartment?.Name ?? "None"} department";
                        }
                        if (roleChanged)
                        {
                            notificationMsg += $". Your role has been set to {user.Role}";
                        }

                        await _notificationService.NotifyAsync(notificationMsg, user.Username);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If ModelState is invalid, repopulate dropdowns
            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewBag.Roles = Enum.GetValues(typeof(RoleType))
                .Cast<RoleType>()
                .Select(r => new { Id = (int)r, Name = r.ToString() })
                .ToList();

            return View(user);
        }

        // GET: /UserManagement/MoveToDepartment
        public async Task<IActionResult> MoveToDepartment()
        {
            // Get users in holding department
            var holdingDepartment = await _context.Departments
                .FirstOrDefaultAsync(d => d.Name == "Holding");

            if (holdingDepartment == null)
            {
                return View(new List<User>());
            }

            var usersInHolding = await _context.Users
                .Where(u => u.DepartmentId == holdingDepartment.Id)
                .ToListAsync();

            // Get all departments for dropdown
            ViewBag.Departments = await _context.Departments
                .Where(d => d.Name != "Holding")
                .ToListAsync();

            return View(usersInHolding);
        }

        // POST: /UserManagement/ProcessBulkMove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessBulkMove(Guid[] selectedUsers, Guid targetDepartmentId, RoleType targetRole)
        {
            if (selectedUsers == null || selectedUsers.Length == 0 || targetDepartmentId == Guid.Empty)
            {
                return BadRequest("No users selected or target department not specified");
            }

            var targetDepartment = await _context.Departments.FindAsync(targetDepartmentId);
            if (targetDepartment == null)
            {
                return NotFound("Target department not found");
            }

            foreach (var userId in selectedUsers)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    var oldDepartmentId = user.DepartmentId;
                    var oldRole = user.Role;

                    // Update user's department and role
                    user.DepartmentId = targetDepartmentId;
                    user.Role = targetRole;

                    // Log the change
                    await _auditService.LogActionAsync(
                        "User",
                        user.Id,
                        "DepartmentAssignment",
                        $"User moved from department {oldDepartmentId} to {targetDepartmentId} and role changed from {oldRole} to {targetRole}");

                    // Notify the user
                    await _notificationService.NotifyAsync(
                        $"You have been assigned to the {targetDepartment.Name} department with the role of {targetRole}",
                        user.Username);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}