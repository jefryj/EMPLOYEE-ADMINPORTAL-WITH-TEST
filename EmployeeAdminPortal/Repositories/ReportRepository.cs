using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ReportRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<DepartmentSummaryItem>> GetDepartmentSummaryAsync()
        {
            return await dbContext.Departments.Select(d => new DepartmentSummaryItem(
                    d.DepartmentName,
                    dbContext.Employees.Count(e => e.DepartmentId == d.Id),
                    dbContext.Employees.Where(e => e.DepartmentId == d.Id).Average(e => (decimal?)e.Salary) ?? 0,
                    dbContext.Employees.Where(e => e.DepartmentId == d.Id).Min(e => (decimal?)e.Salary) ?? 0,
                    dbContext.Employees.Where(e => e.DepartmentId == d.Id).Max(e => (decimal?)e.Salary) ?? 0))
                    .ToListAsync();
        }

        public async Task<List<ProjectSummaryItem>> GetProjectSummaryAsync()
        {
            return await dbContext.Projects.Select(p => new ProjectSummaryItem(
                    p.ProjectName,
                    p.ProjectMembersCount,
                    dbContext.Employees.Where(e => e.ProjectId == p.Id).Average(e => (decimal?)e.Salary) ?? 0,
                    dbContext.Employees.Where(e => e.ProjectId == p.Id).Min(e => (decimal?)e.Salary) ?? 0,
                    dbContext.Employees.Where(e => e.ProjectId == p.Id).Max(e => (decimal?)e.Salary) ?? 0))
                    .ToListAsync();
        }
    }
}