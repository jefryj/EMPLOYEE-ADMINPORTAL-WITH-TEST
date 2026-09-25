using EmployeeAdminPortal.Models.Entities;

namespace EmployeeAdminPortal.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<Employee?> FindByEmailAsync(string email);
}