using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext dbContext;

        public DepartmentRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IQueryable<Department> Query()
        {
            return dbContext.Departments;
        }

        public async Task<Department?> FindAsync(int id)
        {
            return await dbContext.Departments.FindAsync(id);
        }

        public async Task<bool> AnyByNameAsync(string departmentName)
        {
            return await dbContext.Departments.AnyAsync(d => d.DepartmentName == departmentName);
        }

        public async Task<bool> AnyByNameExcludingIdAsync(int id,string departmentName)
        {
            return await dbContext.Departments.AnyAsync(d =>d.Id != id && d.DepartmentName == departmentName);
        }

        public async Task AddAsync(Department department)
        {
            await dbContext.Departments.AddAsync(department);
        }

        public void Remove(Department department)
        {
            dbContext.Departments.Remove(department);
        }

        public void Update(Department department)
        {
            dbContext.Departments.Update(department);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}