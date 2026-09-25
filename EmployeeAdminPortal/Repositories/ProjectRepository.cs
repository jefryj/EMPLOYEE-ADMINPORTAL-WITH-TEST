using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ProjectRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IQueryable<Project> Query()
        {
            return dbContext.Projects;
        }

        public async Task<Project?> FindAsync(int id)
        {
            return await dbContext.Projects.FindAsync(id);
        }

        public async Task<bool> AnyByNameAsync(string projectName)
        {
            return await dbContext.Projects.AnyAsync(p => p.ProjectName == projectName);
        }

        public async Task<bool> AnyByNameExcludingIdAsync(int id,string projectName)
        {
            return await dbContext.Projects.AnyAsync(p =>p.Id != id && p.ProjectName == projectName);
        }

        public async Task AddAsync(Project project)
        {
            await dbContext.Projects.AddAsync(project);
        }

        public void Remove(Project project)
        {
            dbContext.Projects.Remove(project);
        }

        public void Update(Project project)
        {
            dbContext.Projects.Update(project);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}