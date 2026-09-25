using EmployeeAdminPortal.Models.Entities;

namespace EmployeeAdminPortal.Repositories.Interfaces;

public interface IDepartmentRepository
{
    IQueryable<Department> Query();
    Task<Department?> FindAsync(int id);
    Task<bool> AnyByNameAsync(string departmentName);
    Task<bool> AnyByNameExcludingIdAsync(int id, string departmentName);
    Task AddAsync(Department department);
    void Remove(Department department);
    void Update(Department department);
    Task SaveChangesAsync();
}