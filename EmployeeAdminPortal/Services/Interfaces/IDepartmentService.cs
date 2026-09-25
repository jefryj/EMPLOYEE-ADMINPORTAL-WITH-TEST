using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;

namespace EmployeeAdminPortal.Services.Interfaces;

public interface IDepartmentService
{
    Task<List<Department>> GetAllDepartmentsAsync(DepartmentSearchDto searchDto);
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<Department> AddDepartmentAsync(DepartmentDto dto, string userName);
    Task<Department?> UpdateDepartmentAsync(int id, DepartmentDto dto, string userName);
    Task<bool> DeleteDepartmentAsync(int id, string userName);
}