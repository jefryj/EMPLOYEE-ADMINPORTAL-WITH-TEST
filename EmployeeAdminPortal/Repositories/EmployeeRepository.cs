using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext dbContext;

        public EmployeeRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IQueryable<Employee> Query()
        {
            return dbContext.Employees;
        }

        public IQueryable<Employee> QueryWithDetails()
        {
            return dbContext.Employees.Include(e => e.Department).Include(e => e.Project);
        }

        public async Task<Employee?> FindAsync(Guid id)
        {
            return await dbContext.Employees.FindAsync(id);
        }

        public async Task<Employee?> FindByEmailAsync(string email)
        {
            return await dbContext.Employees.FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<bool> AnyByEmailAsync(string email)
        {
            return await dbContext.Employees.AnyAsync(e => e.Email == email);
        }

        public async Task AddAsync(Employee employee)
        {
            await dbContext.Employees.AddAsync(employee);
        }

        public void Remove(Employee employee)
        {
            dbContext.Employees.Remove(employee);
        }

        public void Update(Employee employee)
        {
            dbContext.Employees.Update(employee);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}