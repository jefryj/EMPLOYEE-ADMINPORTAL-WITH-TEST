using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories;
using EmployeeAdminPortal.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Tests;

public class EmployeeServiceTests
{
    private static ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new ApplicationDbContext(options);
    }

    private static EmployeeService CreateService(ApplicationDbContext dbContext)
    {
        return new EmployeeService(
            new EmployeeRepository(dbContext),
            new DepartmentRepository(dbContext),
            new ProjectRepository(dbContext),
            new AuditLogRepository(dbContext));
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_ReturnsEmployee_WhenEmployeeExists()
    {
        var dbContext = GetDbContext();
        await dbContext.Departments.AddAsync(new Department { Id = 1, DepartmentName = "IT" });
        var employee = new Employee { Id = Guid.NewGuid(), Name = "Jefry", Email = "jefry@test.com", DepartmentId = 1 };
        await dbContext.Employees.AddAsync(employee);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.GetEmployeeByIdAsync(employee.Id);

        Assert.NotNull(result);
        Assert.Equal("Jefry", result.Name);
    }

    [Fact]
    public async Task GetAllEmployeesAsync_ReturnsEmployees()
    {
        var dbContext = GetDbContext();
        await dbContext.Departments.AddAsync(new Department { Id = 1, DepartmentName = "IT" });
        await dbContext.Employees.AddRangeAsync(
            new Employee { Id = Guid.NewGuid(), Name = "John", Email = "john@test.com", DepartmentId = 1 },
            new Employee { Id = Guid.NewGuid(), Name = "Alex", Email = "alex@test.com", DepartmentId = 1 });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.GetAllEmployeesAsync(new EmployeeSearchDto { PageNumber = 1, PageSize = 10 });

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task AddEmployeeAsync_CreatesEmployee()
    {
        var dbContext = GetDbContext();
        await dbContext.Departments.AddAsync(new Department { Id = 1, DepartmentName = "IT" });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.AddEmployeeAsync(new AddEmployeeDto
        {
            Name = "Jefry",
            Email = "jefry@test.com",
            Phone = "1234567890",
            Salary = 50000,
            DepartmentId = 1,
            Password = "Password@123",
            Role = "Employee"
        }, "admin@test.com");

        Assert.True(result.Success);
        Assert.Single(dbContext.Employees);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task AddEmployeeAsync_ReturnsFailure_WhenEmailAlreadyExists()
    {
        var dbContext = GetDbContext();
        await dbContext.Departments.AddAsync(new Department { Id = 1, DepartmentName = "IT" });
        await dbContext.Employees.AddAsync(new Employee { Id = Guid.NewGuid(), Name = "Existing User", Email = "jefry@test.com", DepartmentId = 1 });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.AddEmployeeAsync(new AddEmployeeDto
        {
            Name = "New User",
            Email = "jefry@test.com",
            Phone = "1234567890",
            Salary = 50000,
            DepartmentId = 1,
            Password = "Password@123",
            Role = "Employee"
        }, "admin@test.com");

        Assert.False(result.Success);
        Assert.Equal("An employee with this email already exists.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_UpdatesEmployee()
    {
        var dbContext = GetDbContext();
        dbContext.Departments.Add(new Department { Id = 1, DepartmentName = "IT" });
        var employeeId = Guid.NewGuid();
        dbContext.Employees.Add(new Employee { Id = employeeId, Name = "Old Name", Email = "old@test.com", Phone = "1111111111", Salary = 10000, DepartmentId = 1 });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.UpdateEmployeeAsync(employeeId, new UpdateEmployeeDto
        {
            Name = "New Name",
            Email = "new@test.com",
            Phone = "1234567890",
            Salary = 50000,
            DepartmentId = 1
        }, "admin@test.com");

        Assert.True(result.Success);
        Assert.Equal("New Name", result.Employee!.Name);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_ReturnsFailure_WhenEmployeeNotFound()
    {
        var dbContext = GetDbContext();
        dbContext.Departments.Add(new Department { Id = 1, DepartmentName = "IT" });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.UpdateEmployeeAsync(Guid.NewGuid(), new UpdateEmployeeDto
        {
            Name = "Jefry",
            Email = "jefry@test.com",
            Phone = "1234567890",
            Salary = 50000,
            DepartmentId = 1
        }, "admin@test.com");

        Assert.False(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_RemovesEmployee()
    {
        var dbContext = GetDbContext();
        dbContext.Departments.Add(new Department { Id = 1, DepartmentName = "IT" });
        var employeeId = Guid.NewGuid();
        dbContext.Employees.Add(new Employee { Id = employeeId, Name = "Jefry", Email = "jefry@test.com", DepartmentId = 1 });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.DeleteEmployeeAsync(employeeId, "admin@test.com");

        Assert.True(result);
        Assert.Empty(dbContext.Employees);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ReturnsFalse_WhenEmployeeNotFound()
    {
        var service = CreateService(GetDbContext());

        var result = await service.DeleteEmployeeAsync(Guid.NewGuid(), "admin@test.com");

        Assert.False(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_ChangesPasswordSuccessfully()
    {
        var dbContext = GetDbContext();
        var employee = new Employee { Id = Guid.NewGuid(), Name = "Jefry", Email = "jefry@test.com", Role = "Employee" };
        var hasher = new PasswordHasher<Employee>();
        employee.PasswordHash = hasher.HashPassword(employee, "OldPassword123");
        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.ChangePasswordAsync("jefry@test.com", new ChangePasswordDto { CurrentPassword = "OldPassword123", NewPassword = "NewPassword123" });

        Assert.True(result.Success);
        Assert.Equal("Password changed successfully.", result.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_ReturnsBadRequest_WhenCurrentPasswordIsWrong()
    {
        var dbContext = GetDbContext();
        var employee = new Employee { Id = Guid.NewGuid(), Name = "Jefry", Email = "jefry@test.com" };
        var hasher = new PasswordHasher<Employee>();
        employee.PasswordHash = hasher.HashPassword(employee, "OldPassword123");
        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.ChangePasswordAsync("jefry@test.com", new ChangePasswordDto { CurrentPassword = "WrongPassword", NewPassword = "NewPassword123" });

        Assert.False(result.Success);
        Assert.Equal("Current password is incorrect.", result.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_ReturnsNotFound_WhenEmployeeDoesNotExist()
    {
        var service = CreateService(GetDbContext());

        var result = await service.ChangePasswordAsync("missing@test.com", new ChangePasswordDto { CurrentPassword = "OldPassword123", NewPassword = "NewPassword123" });

        Assert.False(result.Success);
        Assert.Equal("Employee not found", result.Message);
    }
}