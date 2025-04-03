using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnterprisePMO_PWA.Domain.Entities;

namespace EnterprisePMO_PWA.Application.Services
{
    public interface IProjectService
    {
        Task<Project> GetProjectByIdAsync(int id);
        Task<List<Project>> GetAllProjectsAsync();
        Task<List<Project>> GetProjectsByDepartmentAsync(int departmentId);
        Task<List<Project>> GetProjectsByManagerAsync(int managerId);
        Task<Project> CreateProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(Project project);
        Task<bool> DeleteProjectAsync(int id);
        Task<List<ProjectTask>> GetProjectTasksAsync(int projectId);
        Task<List<ProjectDependency>> GetProjectDependenciesAsync(int projectId);
        Task<List<ProjectRisk>> GetProjectRisksAsync(int projectId);
        Task<List<ChangeRequest>> GetProjectChangeRequestsAsync(int projectId);
        Task<List<Document>> GetProjectDocumentsAsync(int projectId);
        Task<List<WeeklyUpdate>> GetProjectWeeklyUpdatesAsync(int projectId);
        Task<List<ProjectMember>> GetProjectMembersAsync(int projectId);
    }
} 