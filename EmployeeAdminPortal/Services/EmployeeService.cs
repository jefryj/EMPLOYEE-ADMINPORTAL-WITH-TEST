using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories.Interfaces;
using EmployeeAdminPortal.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Services;

public class EmployeeService(
    IEmployeeRepository employeeRepository,
    IDepartmentRepository departmentRepository,
    IProjectRepository projectRepository,
    IAuditLogRepository auditLogRepository) : IEmployeeService
{
    public async Task<List<Employee>> GetAllEmployeesAsync(EmployeeSearchDto searchDto)
    {
        IQueryable<Employee> query = employeeRepository.QueryWithDetails();

        if (!string.IsNullOrWhiteSpace(searchDto.Search))
        {
            query = query.Where(e => e.Name.Contains(searchDto.Search) || e.Email.Contains(searchDto.Search));
        }

        if (searchDto.DepartmentId.HasValue)
        {
            query = query.Where(e => e.DepartmentId == searchDto.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchDto.SortBy))
        {
            switch (searchDto.SortBy.ToLower())
            {
                case "name":
                    query = searchDto.Descending ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name);
                    break;
                case "email":
                    query = searchDto.Descending ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email);
                    break;
                case "salary":
                    query = searchDto.Descending ? query.OrderByDescending(e => e.Salary) : query.OrderBy(e => e.Salary);
                    break;
                case "department":
                    query = query.OrderBy(e => e.Department.DepartmentName);
                    break;
                default:
                    query = searchDto.Descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id);
                    break;
            }
        }

        return await query.Skip((searchDto.PageNumber - 1) * searchDto.PageSize).Take(searchDto.PageSize)
            .ToListAsync();
    }

    public Task<Employee?> GetEmployeeByIdAsync(Guid id) => employeeRepository.QueryWithDetails().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<EmployeeCommandResult> AddEmployeeAsync(AddEmployeeDto addEmployeeDto, string userName)
    {
        var department = await departmentRepository.FindAsync(addEmployeeDto.DepartmentId);
        if (department is null)
        {
            return new EmployeeCommandResult(false, "Invalid department ID.");
        }

        if (await employeeRepository.AnyByEmailAsync(addEmployeeDto.Email))
        {
            return new EmployeeCommandResult(false, "An employee with this email already exists.");
        }

        if (addEmployeeDto.ProjectId is not null)
        {
            var project = await projectRepository.FindAsync(addEmployeeDto.ProjectId.Value);
            if (project is null)
            {
                return new EmployeeCommandResult(false, "Invalid ProjectId.");
            }

            project.ProjectMembersCount++;
            projectRepository.Update(project);
        }

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = addEmployeeDto.Name,
            Email = addEmployeeDto.Email,
            Phone = addEmployeeDto.Phone,
            Salary = addEmployeeDto.Salary,
            DepartmentId = addEmployeeDto.DepartmentId,
            ProjectId = addEmployeeDto.ProjectId,
            Role = addEmployeeDto.Role
        };

        var hasher = new PasswordHasher<Employee>();
        employee.PasswordHash = hasher.HashPassword(employee, addEmployeeDto.Password);

        await employeeRepository.AddAsync(employee);
        await employeeRepository.SaveChangesAsync();

        await AddAuditLogAsync(userName, "AddEmployee", "Employee", $"Added employee with Id: {employee.Id}, Name: {employee.Name}");

        return new EmployeeCommandResult(true, Employee: employee);
    }

    public async Task<EmployeeCommandResult> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto updateEmployeeDto, string userName)
    {
        var department = await departmentRepository.FindAsync(updateEmployeeDto.DepartmentId);
        if (department is null)
        {
            return new EmployeeCommandResult(false, "Invalid DepartmentId.");
        }

        if (updateEmployeeDto.ProjectId is not null)
        {
            var project = await projectRepository.FindAsync(updateEmployeeDto.ProjectId.Value);
            if (project is null)
            {
                return new EmployeeCommandResult(false, "Invalid ProjectId.");
            }
        }

        var employee = await employeeRepository.FindAsync(id);
        if (employee is null)
        {
            return new EmployeeCommandResult(false);
        }

        var oldProjectId = employee.ProjectId;

        employee.Name = updateEmployeeDto.Name;
        employee.Email = updateEmployeeDto.Email;
        employee.Phone = updateEmployeeDto.Phone;
        employee.Salary = updateEmployeeDto.Salary;
        employee.DepartmentId = updateEmployeeDto.DepartmentId;
        employee.ProjectId = updateEmployeeDto.ProjectId;

        if (oldProjectId != updateEmployeeDto.ProjectId)
        {
            if (oldProjectId is not null)
            {
                var oldProject = await projectRepository.FindAsync(oldProjectId.Value);
                if (oldProject is not null)
                {
                    oldProject.ProjectMembersCount--;
                    projectRepository.Update(oldProject);
                }
            }

            if (updateEmployeeDto.ProjectId is not null)
            {
                var newProject = await projectRepository.FindAsync(updateEmployeeDto.ProjectId.Value);
                if (newProject is not null)
                {
                    newProject.ProjectMembersCount++;
                    projectRepository.Update(newProject);
                }
            }
        }

        employeeRepository.Update(employee);
        await employeeRepository.SaveChangesAsync();

        await AddAuditLogAsync(userName, "UpdateEmployee", "Employee", $"Updated employee with Id: {employee.Id}, Name: {employee.Name}");

        return new EmployeeCommandResult(true, Employee: employee);
    }

    public async Task<bool> DeleteEmployeeAsync(Guid id, string userName)
    {
        var employee = await employeeRepository.FindAsync(id);
        if (employee is null)
        {
            return false;
        }

        if (employee.ProjectId is not null)
        {
            var project = await projectRepository.FindAsync(employee.ProjectId.Value);
            if (project is not null)
            {
                project.ProjectMembersCount--;
                projectRepository.Update(project);
            }
        }

        employeeRepository.Remove(employee);
        await employeeRepository.SaveChangesAsync();

        await AddAuditLogAsync(userName, "DeleteEmployee", "Employee", $"Deleted employee with Id: {employee.Id}, Name: {employee.Name}");

        return true;
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(string? email, ChangePasswordDto request)
    {
        var employee = await employeeRepository.FindByEmailAsync(email ?? string.Empty);
        if (employee is null)
        {
            return (false, "Employee not found");
        }

        var hasher = new PasswordHasher<Employee>();
        var verifyResult = hasher.VerifyHashedPassword(employee, employee.PasswordHash, request.CurrentPassword);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return (false, "Current password is incorrect.");
        }

        employee.PasswordHash = hasher.HashPassword(employee, request.NewPassword);
        employeeRepository.Update(employee);
        await employeeRepository.SaveChangesAsync();

        return (true, "Password changed successfully.");
    }

    private async Task AddAuditLogAsync(string userName, string action, string entityName, string details)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            UserName = userName,
            Action = action,
            EntityName = entityName,
            Details = details,
            CreatedAt = DateTime.UtcNow
        });

        await auditLogRepository.SaveChangesAsync();
    }
}