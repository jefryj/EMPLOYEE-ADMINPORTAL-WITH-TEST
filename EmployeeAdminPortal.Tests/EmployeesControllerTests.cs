using EmployeeAdminPortal.Controllers;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace EmployeeAdminPortal.Tests.Controllers;

public class EmployeesControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        return new ApplicationDbContext(options);
    }

    private EmployeesController GetController(ApplicationDbContext dbContext)
    {
        var logger = Mock.Of<ILogger<EmployeesController>>();

        return new EmployeesController(dbContext,logger);
    }

    [Fact]
    public async Task GetAllEmployeesById_ReturnsEmployee_WhenEmployeeExists()
    {
        var dbContext = GetDbContext();

        var department = new Department
        {
            Id = 1,
            DepartmentName = "IT"
        };

        await dbContext.Departments.AddAsync(department);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = "Jefry",
            Email = "jefry@test.com",
            DepartmentId = 1
        };

        await dbContext.Employees.AddAsync(employee);

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var result = await controller.GetAllEmployeesById(employee.Id);

        var okResult = Assert.IsType<OkObjectResult>(result);

        var returnedEmployee = Assert.IsType<Employee>(okResult.Value);

        Assert.Equal("Jefry",returnedEmployee.Name);
    }

    [Fact]
    public async Task GetAllEmployeesById_ThrowsException_WhenEmployeeNotFound()
    {
        var dbContext = GetDbContext();

        var controller = GetController(dbContext);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => controller.GetAllEmployeesById(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllEmployees_ReturnsEmployees()
    {

        var dbContext = GetDbContext();

        var department = new Department
        {
            Id = 1,
            DepartmentName = "IT"
        };

        await dbContext.Departments.AddAsync(department);

        await dbContext.Employees.AddAsync(new Employee
        {
            Id = Guid.NewGuid(),
            Name = "John",
            Email = "john@test.com",
            DepartmentId = 1
        });

        await dbContext.Employees.AddAsync(new Employee
        {
            Id = Guid.NewGuid(),
            Name = "Alex",
            Email = "alex@test.com",
            DepartmentId = 1
        });

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var searchDto = new EmployeeSearchDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await controller.GetAllEmployees(searchDto);

        var okResult = Assert.IsType<OkObjectResult>(result);

        var employees = Assert.IsAssignableFrom<List<Employee>>(okResult.Value);

        Assert.Equal(2, employees.Count);
    }

    [Fact]
    public async Task AddEmployee_CreatesEmployee()
    {
  
        var dbContext = GetDbContext();

        await dbContext.Departments.AddAsync(
            new Department
            {
                Id = 1,
                DepartmentName = "IT"
            });

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.Email,
                        "admin@test.com")
                },
                "TestAuth"));

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User = user
                    }
            };

        var dto = new AddEmployeeDto
        {
            Name = "Jefry",
            Email = "jefry@test.com",
            Phone = "1234567890",
            Salary = 50000,
            DepartmentId = 1,
            Password = "Password@123",
            Role = "Employee"
        };


        var result = await controller.AddEmployee(dto);

        var createdResult =Assert.IsType<CreatedAtActionResult>(result);

        Assert.Single(dbContext.Employees);

        Assert.Single(dbContext.AuditLogs);
    }
[Fact]
public async Task UpdateEmployee_UpdatesEmployee()
{
    var dbContext = GetDbContext();

    dbContext.Departments.Add(new Department
    {
        Id = 1,
        DepartmentName = "IT"
    });

    var employeeId = Guid.NewGuid();

    dbContext.Employees.Add(new Employee
    {
        Id = employeeId,
        Name = "Old Name",
        Email = "old@test.com",
        Phone = "1111111111",
        Salary = 10000,
        DepartmentId = 1
    });

    await dbContext.SaveChangesAsync();

    var controller = GetController(dbContext);

    var user = new ClaimsPrincipal(
        new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Email, "admin@test.com")
            },
            "TestAuth"));

    controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext
        {
            User = user
        }
    };

    var dto = new UpdateEmployeeDto
    {
        Name = "New Name",
        Email = "new@test.com",
        Phone = "1234567890",
        Salary = 50000,
        DepartmentId = 1
    };

    var result = await controller.UpdateEmployee(employeeId, dto);

    var okResult = Assert.IsType<OkObjectResult>(result);

    var employee = Assert.IsType<Employee>(okResult.Value);

    Assert.Equal("New Name", employee.Name);
    Assert.Equal("new@test.com", employee.Email);
    Assert.Equal("1234567890", employee.Phone);
    Assert.Equal(50000, employee.Salary);
    Assert.Equal(1, employee.DepartmentId);

    Assert.Single(dbContext.AuditLogs);
}
    [Fact]
    public async Task UpdateEmployee_ThrowsException_WhenEmployeeNotFound()
    {
        var dbContext = GetDbContext();
        
        dbContext.Departments.Add(new Department
        {
            Id = 1,
            DepartmentName = "IT"
        });
        await dbContext.SaveChangesAsync();
        var controller = GetController(dbContext);

        var dto = new UpdateEmployeeDto
        {
            Name = "Jefry",
            Email = "jefry@test.com",
            Phone = "1234567890",
            Salary = 50000,
            DepartmentId = 1
           
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => controller.UpdateEmployee(Guid.NewGuid(), dto));
    }


[Fact]
public async Task DeleteEmployee_RemovesEmployee()
{
var dbContext = GetDbContext();

dbContext.Departments.Add(new Department
{
Id = 1,
DepartmentName = "IT"
});

var employeeId = Guid.NewGuid();

dbContext.Employees.Add(new Employee
{
Id = employeeId,
Name = "Jefry",
Email = "jefry@test.com",
DepartmentId = 1
});

await dbContext.SaveChangesAsync();

var controller = GetController(dbContext);

var user = new ClaimsPrincipal(
new ClaimsIdentity(
new[]
{
new Claim(ClaimTypes.Email,"admin@test.com")
},
"TestAuth"));

controller.ControllerContext = new ControllerContext
{
HttpContext = new DefaultHttpContext
{
User = user
}
};

var result = await controller.DeleteEmployee(employeeId);

Assert.IsType<NoContentResult>(result);

Assert.Empty(dbContext.Employees);

Assert.Single(dbContext.AuditLogs);
}

[Fact]
public async Task DeleteEmployee_ThrowsException_WhenEmployeeNotFound()
{
var dbContext = GetDbContext();

var controller = GetController(dbContext);

await Assert.ThrowsAsync<KeyNotFoundException>(
() => controller.DeleteEmployee(Guid.NewGuid()));
}
}