using System;
using System.Linq;
using EnterprisePMO_PWA.Domain.Entities;

namespace EnterprisePMO_PWA.Infrastructure.Data
{
    /// <summary>
    /// Seeds initial data (departments, users, and roles) if no data exists.
    /// </summary>
    public static class DataSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // For in-memory database, we don't need to EnsureCreated
            // Just check if any data exists
            if (!context.Departments.Any())
            {
                SeedDepartments(context);
                SeedRoles(context);
                SeedUsers(context);
                SeedStrategicGoals(context);
                SeedAnnualGoals(context);
                SeedExampleProjects(context);
                
                context.SaveChanges();
            }
        }

        private static void SeedDepartments(AppDbContext context)
        {
            var departments = new[]
            {
                new Department { Id = Guid.NewGuid(), Name = "Holding" }, // Added holding department
                new Department { Id = Guid.NewGuid(), Name = "IT" },
                new Department { Id = Guid.NewGuid(), Name = "Finance" },
                new Department { Id = Guid.NewGuid(), Name = "Marketing" },
                new Department { Id = Guid.NewGuid(), Name = "Operations" },
                new Department { Id = Guid.NewGuid(), Name = "Human Resources" }
            };

            context.Departments.AddRange(departments);
            context.SaveChanges();
        }

        private static void SeedRoles(AppDbContext context)
        {
            var roles = new[]
            {
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "Admin", 
                    Description = "Administrator with full system access",
                    HierarchyLevel = 100,
                    CanManageProjects = true,
                    CanManageUsers = true,
                    CanApproveRequests = true,
                    CanManageRoles = true,
                    CanViewReports = true,
                    CanViewAuditLogs = true
                },
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "Main PMO", 
                    Description = "Main Project Management Office",
                    HierarchyLevel = 90,
                    CanManageProjects = true,
                    CanManageUsers = false,
                    CanApproveRequests = true,
                    CanManageRoles = false,
                    CanViewReports = true,
                    CanViewAuditLogs = true
                },
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "Executive", 
                    Description = "Executive leadership",
                    HierarchyLevel = 80,
                    CanManageProjects = false,
                    CanManageUsers = false,
                    CanApproveRequests = false,
                    CanManageRoles = false,
                    CanViewReports = true,
                    CanViewAuditLogs = false
                },
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "Department Director", 
                    Description = "Department director/manager",
                    HierarchyLevel = 70,
                    CanManageProjects = false,
                    CanManageUsers = false,
                    CanApproveRequests = false,
                    CanManageRoles = false,
                    CanViewReports = true,
                    CanViewAuditLogs = false
                },
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "Sub PMO", 
                    Description = "Sub-level Project Management Office",
                    HierarchyLevel = 60,
                    CanManageProjects = true,
                    CanManageUsers = false,
                    CanApproveRequests = true,
                    CanManageRoles = false,
                    CanViewReports = true,
                    CanViewAuditLogs = false
                },
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "Project Lead", 
                    Description = "Leads the project team",
                    HierarchyLevel = 50,
                    CanManageProjects = true,
                    CanManageUsers = false,
                    CanApproveRequests = false,
                    CanManageRoles = false,
                    CanViewReports = true,
                    CanViewAuditLogs = false
                },
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "Developer", 
                    Description = "Implements technical components",
                    HierarchyLevel = 40,
                    CanManageProjects = false,
                    CanManageUsers = false,
                    CanApproveRequests = false,
                    CanManageRoles = false,
                    CanViewReports = false,
                    CanViewAuditLogs = false
                },
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "Designer", 
                    Description = "Responsible for UX/UI design",
                    HierarchyLevel = 40,
                    CanManageProjects = false,
                    CanManageUsers = false,
                    CanApproveRequests = false,
                    CanManageRoles = false,
                    CanViewReports = false,
                    CanViewAuditLogs = false
                },
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "Tester", 
                    Description = "Performs quality assurance",
                    HierarchyLevel = 40,
                    CanManageProjects = false,
                    CanManageUsers = false,
                    CanApproveRequests = false,
                    CanManageRoles = false,
                    CanViewReports = false,
                    CanViewAuditLogs = false
                },
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "Business Analyst", 
                    Description = "Analyzes business requirements",
                    HierarchyLevel = 40,
                    CanManageProjects = false,
                    CanManageUsers = false,
                    CanApproveRequests = false,
                    CanManageRoles = false,
                    CanViewReports = true,
                    CanViewAuditLogs = false
                },
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "Stakeholder", 
                    Description = "Key business stakeholder",
                    HierarchyLevel = 30,
                    CanManageProjects = false,
                    CanManageUsers = false,
                    CanApproveRequests = false,
                    CanManageRoles = false,
                    CanViewReports = true,
                    CanViewAuditLogs = false
                },
                new Role { 
                    Id = Guid.NewGuid(), 
                    RoleName = "New User", 
                    Description = "Recently registered user awaiting department assignment",
                    HierarchyLevel = 10,
                    CanManageProjects = false,
                    CanManageUsers = false,
                    CanApproveRequests = false,
                    CanManageRoles = false,
                    CanViewReports = false,
                    CanViewAuditLogs = false
                }
            };

            context.Roles.AddRange(roles);
            context.SaveChanges();
        }

        private static void SeedUsers(AppDbContext context)
        {
            // Get the departments for reference
            var holdingDepartment = context.Departments.FirstOrDefault(d => d.Name == "Holding");
            var itDepartment = context.Departments.FirstOrDefault(d => d.Name == "IT");
            var financeDepartment = context.Departments.FirstOrDefault(d => d.Name == "Finance");
            var marketingDepartment = context.Departments.FirstOrDefault(d => d.Name == "Marketing");
            var operationsDepartment = context.Departments.FirstOrDefault(d => d.Name == "Operations");
            var hrDepartment = context.Departments.FirstOrDefault(d => d.Name == "Human Resources");

            var users = new[]
            {
                // Project Managers
                new User { Id = Guid.NewGuid(), Username = "pm1@company.com", Role = RoleType.ProjectManager, DepartmentId = itDepartment?.Id },
                new User { Id = Guid.NewGuid(), Username = "pm2@company.com", Role = RoleType.ProjectManager, DepartmentId = financeDepartment?.Id },
                new User { Id = Guid.NewGuid(), Username = "pm3@company.com", Role = RoleType.ProjectManager, DepartmentId = marketingDepartment?.Id },
                
                // Sub PMOs
                new User { Id = Guid.NewGuid(), Username = "subpmo1@company.com", Role = RoleType.SubPMO, DepartmentId = itDepartment?.Id },
                new User { Id = Guid.NewGuid(), Username = "subpmo2@company.com", Role = RoleType.SubPMO, DepartmentId = financeDepartment?.Id },
                
                // Main PMO
                new User { Id = Guid.NewGuid(), Username = "mainpmo@company.com", Role = RoleType.MainPMO },
                
                // Department Directors
                new User { Id = Guid.NewGuid(), Username = "itdirector@company.com", Role = RoleType.DepartmentDirector, DepartmentId = itDepartment?.Id },
                new User { Id = Guid.NewGuid(), Username = "financedirector@company.com", Role = RoleType.DepartmentDirector, DepartmentId = financeDepartment?.Id },
                
                // Executives
                new User { Id = Guid.NewGuid(), Username = "executive@company.com", Role = RoleType.Executive },
                
                // Admin
                new User { Id = Guid.NewGuid(), Username = "admin@company.com", Role = RoleType.Admin },

                // New user in holding department
                new User { Id = Guid.NewGuid(), Username = "newuser@company.com", Role = RoleType.ProjectManager, DepartmentId = holdingDepartment?.Id }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        private static void SeedStrategicGoals(AppDbContext context)
        {
            var strategicGoals = new[]
            {
                new StrategicGoal { Id = Guid.NewGuid(), Name = "Digital Transformation", Description = "Transform business processes through digital technologies" },
                new StrategicGoal { Id = Guid.NewGuid(), Name = "Customer Experience", Description = "Enhance customer experience across all touchpoints" },
                new StrategicGoal { Id = Guid.NewGuid(), Name = "Operational Excellence", Description = "Optimize operations for efficiency and quality" },
                new StrategicGoal { Id = Guid.NewGuid(), Name = "Market Expansion", Description = "Expand into new markets and segments" }
            };

            context.StrategicGoals.AddRange(strategicGoals);
            context.SaveChanges();
        }

        private static void SeedAnnualGoals(AppDbContext context)
        {
            // Get strategic goals for reference
            var digitalTransformation = context.StrategicGoals.FirstOrDefault(g => g.Name == "Digital Transformation");
            var customerExperience = context.StrategicGoals.FirstOrDefault(g => g.Name == "Customer Experience");
            var operationalExcellence = context.StrategicGoals.FirstOrDefault(g => g.Name == "Operational Excellence");
            var marketExpansion = context.StrategicGoals.FirstOrDefault(g => g.Name == "Market Expansion");

            var annualGoals = new[]
            {
                new AnnualGoal { Id = Guid.NewGuid(), Name = "Cloud Migration", Description = "Migrate key applications to the cloud", StrategicGoalId = digitalTransformation?.Id },
                new AnnualGoal { Id = Guid.NewGuid(), Name = "Mobile App Development", Description = "Develop mobile applications for customers", StrategicGoalId = customerExperience?.Id },
                new AnnualGoal { Id = Guid.NewGuid(), Name = "Process Automation", Description = "Automate manual business processes", StrategicGoalId = operationalExcellence?.Id },
                new AnnualGoal { Id = Guid.NewGuid(), Name = "E-commerce Platform", Description = "Launch new e-commerce platform", StrategicGoalId = marketExpansion?.Id }
            };

            context.AnnualGoals.AddRange(annualGoals);
            context.SaveChanges();
        }

        private static void SeedExampleProjects(AppDbContext context)
        {
            // Get departments for reference
            var itDepartment = context.Departments.FirstOrDefault(d => d.Name == "IT");
            var marketingDepartment = context.Departments.FirstOrDefault(d => d.Name == "Marketing");
            
            // Get project managers for reference
            var itProjectManager = context.Users.FirstOrDefault(u => u.Username == "pm1@company.com");
            var marketingProjectManager = context.Users.FirstOrDefault(u => u.Username == "pm3@company.com");
            
            // Get strategic goals for reference
            var digitalTransformation = context.StrategicGoals.FirstOrDefault(g => g.Name == "Digital Transformation");
            var customerExperience = context.StrategicGoals.FirstOrDefault(g => g.Name == "Customer Experience");
            
            // Get annual goals for reference
            var cloudMigration = context.AnnualGoals.FirstOrDefault(g => g.Name == "Cloud Migration");
            var mobileApp = context.AnnualGoals.FirstOrDefault(g => g.Name == "Mobile App Development");

            if (itDepartment != null && itProjectManager != null && digitalTransformation != null && cloudMigration != null &&
                marketingDepartment != null && marketingProjectManager != null && customerExperience != null && mobileApp != null)
            {
                var projects = new[]
                {
                    new Project
                    {
                        Id = Guid.NewGuid(),
                        Name = "ERP System Migration",
                        Description = "Migrate the current ERP system to a cloud-based solution",
                        DepartmentId = itDepartment.Id,
                        ProjectManagerId = itProjectManager.Id,
                        StrategicGoalId = digitalTransformation.Id,
                        AnnualGoalId = cloudMigration.Id,
                        StartDate = DateTime.UtcNow.AddDays(-30),
                        EndDate = DateTime.UtcNow.AddDays(120),
                        Budget = 500000,
                        ActualCost = 150000,
                        EstimatedCost = 480000,
                        ClientName = "Internal IT",
                        StatusColor = StatusColor.Green,
                        Status = ProjectStatus.Active,
                        CreationDate = DateTime.UtcNow.AddDays(-45),
                        ApprovedDate = DateTime.UtcNow.AddDays(-35),
                        Category = "IT Infrastructure"
                    },
                    new Project
                    {
                        Id = Guid.NewGuid(),
                        Name = "Customer Mobile App",
                        Description = "Develop a new mobile app for customer self-service",
                        DepartmentId = marketingDepartment.Id,
                        ProjectManagerId = marketingProjectManager.Id,
                        StrategicGoalId = customerExperience.Id,
                        AnnualGoalId = mobileApp.Id,
                        StartDate = DateTime.UtcNow.AddDays(-60),
                        EndDate = DateTime.UtcNow.AddDays(30),
                        Budget = 350000,
                        ActualCost = 300000,
                        EstimatedCost = 340000,
                        ClientName = "Marketing Division",
                        StatusColor = StatusColor.Yellow,
                        Status = ProjectStatus.Active,
                        CreationDate = DateTime.UtcNow.AddDays(-75),
                        ApprovedDate = DateTime.UtcNow.AddDays(-65),
                        Category = "Software Development"
                    }
                };

                context.Projects.AddRange(projects);
                context.SaveChanges();

                // Add example weekly updates for the projects
                var erpProject = projects[0];
                var mobileProject = projects[1];

                var weeklyUpdates = new[]
                {
                    new WeeklyUpdate
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = erpProject.Id,
                        WeekEndingDate = DateTime.UtcNow.AddDays(-7),
                        Accomplishments = "Completed data migration plan and initial testing",
                        NextSteps = "Begin pilot migration with non-critical data",
                        IssuesOrRisks = "None at this time",
                        PercentComplete = 25,
                        StatusColor = StatusColor.Green,
                        IsApprovedBySubPMO = true,
                        IsSentBack = false,
                        SubmittedByUserId = itProjectManager.Id,
                        SubmittedDate = DateTime.UtcNow.AddDays(-7),
                        ApprovedDate = DateTime.UtcNow.AddDays(-6)
                    },
                    new WeeklyUpdate
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = mobileProject.Id,
                        WeekEndingDate = DateTime.UtcNow.AddDays(-7),
                        Accomplishments = "UI design complete, began backend API development",
                        NextSteps = "Continue API development and start frontend implementation",
                        IssuesOrRisks = "Potential delay in third-party integration",
                        PercentComplete = 65,
                        StatusColor = StatusColor.Yellow,
                        IsApprovedBySubPMO = true,
                        IsSentBack = false,
                        SubmittedByUserId = marketingProjectManager.Id,
                        SubmittedDate = DateTime.UtcNow.AddDays(-7),
                        ApprovedDate = DateTime.UtcNow.AddDays(-6)
                    }
                };

                context.WeeklyUpdates.AddRange(weeklyUpdates);
                context.SaveChanges();
            }
        }
    }
}