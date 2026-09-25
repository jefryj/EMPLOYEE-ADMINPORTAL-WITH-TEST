using EmployeeAdminPortal.Controllers;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace EmployeeAdminPortal.Tests.Controllers;

public class EmployeesControllerTests
{
    private static EmployeesController CreateController(Mock<IEmployeeService> service)
    {
        return new EmployeesController(service.Object, Mock.Of<ILogger<EmployeesController>>());
    }

    private static void SetUser(ControllerBase controller, string email)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) }, "TestAuth"))
            }
        };
    }

    [Fact]
    public async Task GetAllEmployeesById_ReturnsEmployee_WhenEmployeeExists()
    {
        var employee = new Employee { Id = Guid.NewGuid(), Name = "Jefry", Email = "jefry@test.com", DepartmentId = 1 };
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.GetEmployeeByIdAsync(employee.Id)).ReturnsAsync(employee);
        var controller = CreateController(service);

        var result = await controller.GetAllEmployeesById(employee.Id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEmployee = Assert.IsType<Employee>(okResult.Value);
        Assert.Equal("Jefry", returnedEmployee.Name);
    }

    [Fact]
    public async Task GetAllEmployeesById_ThrowsException_WhenEmployeeNotFound()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.GetEmployeeByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Employee?)null);
        var controller = CreateController(service);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => controller.GetAllEmployeesById(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllEmployees_ReturnsEmployees()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.GetAllEmployeesAsync(It.IsAny<EmployeeSearchDto>())).ReturnsAsync([
            new Employee { Id = Guid.NewGuid(), Name = "John", Email = "john@test.com", DepartmentId = 1 },
            new Employee { Id = Guid.NewGuid(), Name = "Alex", Email = "alex@test.com", DepartmentId = 1 }
        ]);
        var controller = CreateController(service);

        var result = await controller.GetAllEmployees(new EmployeeSearchDto { PageNumber = 1, PageSize = 10 });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var employees = Assert.IsAssignableFrom<List<Employee>>(okResult.Value);
        Assert.Equal(2, employees.Count);
    }

    [Fact]
    public async Task AddEmployee_CreatesEmployee()
    {
        var employee = new Employee { Id = Guid.NewGuid(), Name = "Jefry", Email = "jefry@test.com", DepartmentId = 1 };
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.AddEmployeeAsync(It.IsAny<AddEmployeeDto>(), It.IsAny<string>())).ReturnsAsync(new EmployeeCommandResult(true, null, employee));
        var controller = CreateController(service);
        SetUser(controller, "admin@test.com");

        var result = await controller.AddEmployee(new AddEmployeeDto
        {
            Name = "Jefry",
            Email = "jefry@test.com",
            Phone = "1234567890",
            Salary = 50000,
            DepartmentId = 1,
            Password = "Password@123",
            Role = "Employee"
        });

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.IsType<Employee>(createdResult.Value);
    }

    [Fact]
    public async Task AddEmployee_ReturnsBadRequest_WhenEmailAlreadyExists()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.AddEmployeeAsync(It.IsAny<AddEmployeeDto>(), It.IsAny<string>())).ReturnsAsync(new EmployeeCommandResult(false, "An employee with this email already exists."));
        var controller = CreateController(service);
        SetUser(controller, "admin@test.com");

        var result = await controller.AddEmployee(new AddEmployeeDto
        {
            Name = "New User",
            Email = "jefry@test.com",
            Phone = "1234567890",
            Salary = 50000,
            DepartmentId = 1,
            Password = "Password@123",
            Role = "Employee"
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("An employee with this email already exists.", badRequest.Value);
    }

    [Fact]
    public async Task UpdateEmployee_UpdatesEmployee()
    {
        var employee = new Employee { Id = Guid.NewGuid(), Name = "New Name", Email = "new@test.com", Phone = "1234567890", Salary = 50000, DepartmentId = 1 };
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.UpdateEmployeeAsync(employee.Id, It.IsAny<UpdateEmployeeDto>(), It.IsAny<string>())).ReturnsAsync(new EmployeeCommandResult(true, null, employee));
        var controller = CreateController(service);
        SetUser(controller, "admin@test.com");

        var result = await controller.UpdateEmployee(employee.Id, new UpdateEmployeeDto
        {
            Name = "New Name",
            Email = "new@test.com",
            Phone = "1234567890",
            Salary = 50000,
            DepartmentId = 1
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEmployee = Assert.IsType<Employee>(okResult.Value);
        Assert.Equal("New Name", returnedEmployee.Name);
    }

    [Fact]
    public async Task UpdateEmployee_ThrowsException_WhenEmployeeNotFound()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.UpdateEmployeeAsync(It.IsAny<Guid>(), It.IsAny<UpdateEmployeeDto>(), It.IsAny<string>())).ReturnsAsync(new EmployeeCommandResult(false));
        var controller = CreateController(service);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => controller.UpdateEmployee(Guid.NewGuid(), new UpdateEmployeeDto
        {
            Name = "Jefry",
            Email = "jefry@test.com",
            Phone = "1234567890",
            Salary = 50000,
            DepartmentId = 1
        }));
    }

    [Fact]
    public async Task DeleteEmployee_RemovesEmployee()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.DeleteEmployeeAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(true);
        var controller = CreateController(service);
        SetUser(controller, "admin@test.com");

        var result = await controller.DeleteEmployee(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteEmployee_ThrowsException_WhenEmployeeNotFound()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.DeleteEmployeeAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(false);
        var controller = CreateController(service);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => controller.DeleteEmployee(Guid.NewGuid()));
    }

    [Fact]
    public async Task ChangePassword_ChangesPasswordSuccessfully()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.ChangePasswordAsync("jefry@test.com", It.IsAny<ChangePasswordDto>())).ReturnsAsync((true, "Password changed successfully."));
        var controller = CreateController(service);
        SetUser(controller, "jefry@test.com");

        var result = await controller.ChangePassword(new ChangePasswordDto { CurrentPassword = "OldPassword123", NewPassword = "NewPassword123" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Password changed successfully.", okResult.Value);
    }

    [Fact]
    public async Task ChangePassword_ReturnsBadRequest_WhenCurrentPasswordIsWrong()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.ChangePasswordAsync("jefry@test.com", It.IsAny<ChangePasswordDto>())).ReturnsAsync((false, "Current password is incorrect."));
        var controller = CreateController(service);
        SetUser(controller, "jefry@test.com");

        var result = await controller.ChangePassword(new ChangePasswordDto { CurrentPassword = "WrongPassword", NewPassword = "NewPassword123" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Current password is incorrect.", badRequest.Value);
    }

    [Fact]
    public async Task ChangePassword_ReturnsNotFound_WhenEmployeeDoesNotExist()
    {
        var service = new Mock<IEmployeeService>();
        service.Setup(s => s.ChangePasswordAsync("missing@test.com", It.IsAny<ChangePasswordDto>())).ReturnsAsync((false, "Employee not found"));
        var controller = CreateController(service);
        SetUser(controller, "missing@test.com");

        var result = await controller.ChangePassword(new ChangePasswordDto { CurrentPassword = "OldPassword123", NewPassword = "NewPassword123" });

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Employee not found", notFound.Value);
    }
}