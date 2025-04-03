using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;
using EnterprisePMO_PWA.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnterprisePMO_PWA.Infrastructure.Data
{
    /// <summary>
    /// Seeds initial data (departments, users, and roles) if no data exists.
    /// </summary>
    public class DataSeeder
    {
        private readonly AppDbContext _context;

        public DataSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            if (!_context.Roles.Any())
            {
                await SeedRolesAsync();
            }

            if (!_context.Departments.Any())
            {
                await SeedDepartmentsAsync();
            }

            if (!_context.StrategicGoals.Any())
            {
                await SeedStrategicGoalsAsync();
            }

            if (!_context.AnnualGoals.Any())
            {
                await SeedAnnualGoalsAsync();
            }

            if (!_context.Users.Any())
            {
                await SeedUsersAsync();
            }

            if (!_context.Projects.Any())
            {
                await SeedProjectsAsync();
            }
        }

        private async Task SeedRolesAsync()
        {
            var roles = new List<Role>
            {
                new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "Administrator",
                    Description = "Full system access",
                    Type = UserRole.Admin,
                    HierarchyLevel = 100
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "PMO Head",
                    Description = "Project Management Office Head",
                    Type = UserRole.PMOHead,
                    HierarchyLevel = 90
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "Project Manager",
                    Description = "Manages projects and teams",
                    Type = UserRole.ProjectManager,
                    HierarchyLevel = 80
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "Team Member",
                    Description = "Works on project tasks",
                    Type = UserRole.TeamMember,
                    HierarchyLevel = 70
                },
                new Role
                {
                    Id = Guid.NewGuid(),
                    RoleName = "Viewer",
                    Description = "Limited view access",
                    Type = UserRole.Viewer,
                    HierarchyLevel = 60
                }
            };

            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();
        }

        private async Task SeedDepartmentsAsync()
        {
            var departments = new List<Department>
            {
                new Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Information Technology",
                    Description = "IT Department"
                },
                new Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Human Resources",
                    Description = "HR Department"
                },
                new Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Finance",
                    Description = "Finance Department"
                }
            };

            await _context.Departments.AddRangeAsync(departments);
            await _context.SaveChangesAsync();
        }

        private async Task SeedStrategicGoalsAsync()
        {
            var strategicGoals = new List<StrategicGoal>
            {
                new StrategicGoal
                {
                    Id = Guid.NewGuid(),
                    Name = "Digital Transformation",
                    Description = "Transform business processes through digital solutions"
                },
                new StrategicGoal
                {
                    Id = Guid.NewGuid(),
                    Name = "Process Automation",
                    Description = "Automate manual processes to improve efficiency"
                }
            };

            await _context.StrategicGoals.AddRangeAsync(strategicGoals);
            await _context.SaveChangesAsync();
        }

        private async Task SeedAnnualGoalsAsync()
        {
            var strategicGoals = await _context.StrategicGoals.ToListAsync();
            var annualGoals = new List<AnnualGoal>
            {
                new AnnualGoal
                {
                    Id = Guid.NewGuid(),
                    Name = "Improve Project Management",
                    Description = "Enhance project management capabilities",
                    StrategicGoalId = strategicGoals[0].Id
                },
                new AnnualGoal
                {
                    Id = Guid.NewGuid(),
                    Name = "Improve HR Operations",
                    Description = "Streamline HR processes",
                    StrategicGoalId = strategicGoals[1].Id
                }
            };

            await _context.AnnualGoals.AddRangeAsync(annualGoals);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUsersAsync()
        {
            var departments = await _context.Departments.ToListAsync();

            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    Role = UserRole.Admin,
                    DepartmentId = departments[0].Id,
                    SupabaseId = "admin123"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "pmohead",
                    Role = UserRole.PMOHead,
                    DepartmentId = departments[0].Id,
                    SupabaseId = "pmohead123"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "projectmanager1",
                    Role = UserRole.ProjectManager,
                    DepartmentId = departments[0].Id,
                    SupabaseId = "pm1123"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "projectmanager2",
                    Role = UserRole.ProjectManager,
                    DepartmentId = departments[1].Id,
                    SupabaseId = "pm2123"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "teammember1",
                    Role = UserRole.TeamMember,
                    DepartmentId = departments[0].Id,
                    SupabaseId = "tm1123"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "teammember2",
                    Role = UserRole.TeamMember,
                    DepartmentId = departments[1].Id,
                    SupabaseId = "tm2123"
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "viewer1",
                    Role = UserRole.Viewer,
                    DepartmentId = departments[2].Id,
                    SupabaseId = "viewer1123"
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }

        private async Task SeedProjectsAsync()
        {
            var departments = await _context.Departments.ToListAsync();
            var users = await _context.Users.ToListAsync();
            var annualGoals = await _context.AnnualGoals.ToListAsync();
            var roles = await _context.Roles.ToListAsync();

            var projectManager = users.First(u => u.Role == UserRole.ProjectManager);
            var teamMember = users.First(u => u.Role == UserRole.TeamMember);
            var projectManagerRole = roles.First(r => r.Type == UserRole.ProjectManager);
            var teamMemberRole = roles.First(r => r.Type == UserRole.TeamMember);

            var projects = new List<Project>
            {
                new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "Enterprise PMO System",
                    Description = "Development of the Enterprise PMO System",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(6),
                    Status = ProjectStatus.Active,
                    StatusColor = StatusColor.Green,
                    Budget = 100000,
                    ActualCost = 0,
                    EstimatedCost = 90000,
                    ClientName = "Internal",
                    DepartmentId = departments[0].Id,
                    ProjectManagerId = projectManager.Id,
                    StrategicGoalId = annualGoals[0].StrategicGoalId,
                    AnnualGoalId = annualGoals[0].Id,
                    Category = "Software Development",
                    PercentComplete = 0,
                    CreationDate = DateTime.UtcNow,
                    Members = new List<ProjectMember>
                    {
                        new ProjectMember
                        {
                            Id = Guid.NewGuid(),
                            UserId = projectManager.Id,
                            Role = projectManagerRole
                        },
                        new ProjectMember
                        {
                            Id = Guid.NewGuid(),
                            UserId = teamMember.Id,
                            Role = teamMemberRole
                        }
                    }
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Name = "HR System Upgrade",
                    Description = "Upgrade of the HR Management System",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    EndDate = DateTime.UtcNow.AddMonths(4),
                    Status = ProjectStatus.Draft,
                    StatusColor = StatusColor.Yellow,
                    Budget = 50000,
                    ActualCost = 0,
                    EstimatedCost = 45000,
                    ClientName = "HR Department",
                    DepartmentId = departments[1].Id,
                    ProjectManagerId = projectManager.Id,
                    StrategicGoalId = annualGoals[1].StrategicGoalId,
                    AnnualGoalId = annualGoals[1].Id,
                    Category = "System Upgrade",
                    PercentComplete = 0,
                    CreationDate = DateTime.UtcNow,
                    Members = new List<ProjectMember>
                    {
                        new ProjectMember
                        {
                            Id = Guid.NewGuid(),
                            UserId = projectManager.Id,
                            Role = projectManagerRole
                        }
                    }
                }
            };

            await _context.Projects.AddRangeAsync(projects);
            await _context.SaveChangesAsync();
        }
    }
}