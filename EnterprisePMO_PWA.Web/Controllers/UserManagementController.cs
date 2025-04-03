using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EnterprisePMO_PWA.Infrastructure.Data;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Application.Services;
using EnterprisePMO_PWA.Domain.Enums;

namespace EnterprisePMO_PWA.Web.Controllers
{
    [Authorize(Policy = "Permission:ManageUsers")]
    public class UserManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly UserService _userService;
        private readonly RoleService _roleService;

        public UserManagementController(
            AppDbContext context,
            AuditService auditService,
            INotificationService notificationService,
            UserService userService,
            RoleService roleService)
        {
            _context = context;
            _auditService = auditService;
            _notificationService = notificationService;
            _userService = userService;
            _roleService = roleService;
        }

        // GET: /UserManagement
        [HttpGet]
        public IActionResult Index()
        {
            var users = _userService.GetAllUsers();
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
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var user = _userService.GetAllUsers().FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserRole = GetCurrentUserRoleType();
            if (!_roleService.GetManageableRoles(currentUserRole).Any(r => r.RoleName == user.Role.ToString()))
            {
                return Forbid();
            }

            var manageableRoles = _roleService.GetManageableRoles(currentUserRole);
            ViewBag.ManageableRoles = manageableRoles;
            return View(user);
        }

        // POST: /UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            if (ModelState.IsValid)
            {
                var currentUserRole = GetCurrentUserRoleType();
                if (!_roleService.GetManageableRoles(currentUserRole).Any(r => r.RoleName == user.Role.ToString()))
                {
                    ModelState.AddModelError("", "You don't have permission to edit users with this role level.");
                    return View(user);
                }

                var result = await _userService.UpdateUserAsync(user, Guid.Parse(User.Identity?.Name ?? Guid.Empty.ToString()));
                if (result != null)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            var manageableRoles = _roleService.GetManageableRoles(GetCurrentUserRoleType());
            ViewBag.ManageableRoles = manageableRoles;
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
        public async Task<IActionResult> ProcessBulkMove(Guid[] selectedUsers, Guid targetDepartmentId, UserRole targetRole)
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

        [HttpGet]
        public IActionResult Create()
        {
            var currentUserRole = GetCurrentUserRoleType();
            var manageableRoles = _roleService.GetManageableRoles(currentUserRole);
            ViewBag.ManageableRoles = manageableRoles;
            return View(new User());
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                var currentUserRole = GetCurrentUserRoleType();
                if (!_roleService.GetManageableRoles(currentUserRole).Any(r => r.RoleName == user.Role.ToString()))
                {
                    ModelState.AddModelError("", "You don't have permission to create users with this role level.");
                    return View(user);
                }

                await _userService.CreateUserAsync(user, Guid.Parse(User.Identity?.Name ?? Guid.Empty.ToString()));
                return RedirectToAction(nameof(Index));
            }

            var manageableRoles = _roleService.GetManageableRoles(GetCurrentUserRoleType());
            ViewBag.ManageableRoles = manageableRoles;
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = _userService.GetAllUsers().FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserRole = GetCurrentUserRoleType();
            if (!_roleService.GetManageableRoles(currentUserRole).Any(r => r.RoleName == user.Role.ToString()))
            {
                return Forbid();
            }

            await _userService.DeleteUserAsync(id, Guid.Parse(User.Identity?.Name ?? Guid.Empty.ToString()));
            return RedirectToAction(nameof(Index));
        }

        private UserRole GetCurrentUserRoleType()
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Viewer;
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}