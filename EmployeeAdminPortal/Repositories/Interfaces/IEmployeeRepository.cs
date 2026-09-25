using EmployeeAdminPortal.Models.Entities;

namespace EmployeeAdminPortal.Repositories.Interfaces;

public interface IEmployeeRepository
{
    IQueryable<Employee> Query();
    IQueryable<Employee> QueryWithDetails();
    Task<Employee?> FindAsync(Guid id);
    Task<Employee?> FindByEmailAsync(string email);
    Task<bool> AnyByEmailAsync(string email);
    Task AddAsync(Employee employee);
    void Remove(Employee employee);
    void Update(Employee employee);
    Task SaveChangesAsync();
}