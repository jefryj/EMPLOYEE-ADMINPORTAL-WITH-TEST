using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;

namespace EmployeeAdminPortal.Services.Interfaces;

public sealed record EmployeeCommandResult(bool Success, string? ErrorMessage = null, Employee? Employee = null);

public interface IEmployeeService
{
    Task<List<Employee>> GetAllEmployeesAsync(EmployeeSearchDto searchDto);
    Task<Employee?> GetEmployeeByIdAsync(Guid id);
    Task<EmployeeCommandResult> AddEmployeeAsync(AddEmployeeDto addEmployeeDto, string userName);
    Task<EmployeeCommandResult> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto updateEmployeeDto, string userName);
    Task<bool> DeleteEmployeeAsync(Guid id, string userName);
    Task<(bool Success, string Message)> ChangePasswordAsync(string? email, ChangePasswordDto request);
}