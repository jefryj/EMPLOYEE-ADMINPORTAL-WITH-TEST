using EmployeeAdminPortal.Models.Entities;

namespace EmployeeAdminPortal.Repositories.Interfaces;

public interface IProjectRepository
{
    IQueryable<Project> Query();
    Task<Project?> FindAsync(int id);
    Task<bool> AnyByNameAsync(string projectName);
    Task<bool> AnyByNameExcludingIdAsync(int id, string projectName);
    Task AddAsync(Project project);
    void Remove(Project project);
    void Update(Project project);
    Task SaveChangesAsync();
}