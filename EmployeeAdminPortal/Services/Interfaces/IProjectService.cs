using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;

namespace EmployeeAdminPortal.Services.Interfaces;

public interface IProjectService
{
    Task<List<Project>> GetAllProjectsAsync(ProjectSearchDto searchDto);
    Task<Project?> GetProjectByIdAsync(int id);
    Task<Project> AddProjectAsync(ProjectDto dto, string userName);
    Task<Project?> UpdateProjectAsync(int id, ProjectDto dto, string userName);
    Task<bool> DeleteProjectAsync(int id, string userName);
}