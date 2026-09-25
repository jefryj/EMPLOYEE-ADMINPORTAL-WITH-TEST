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

public class DepartmentsControllerTests
{
    private static DepartmentsController CreateController(Mock<IDepartmentService> service)
    {
        return new DepartmentsController(service.Object, Mock.Of<ILogger<DepartmentsController>>());
    }

    private static void SetUser(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "test@test.com") }, "TestAuthentication"))
            }
        };
    }

    [Fact]
    public async Task GetDepartmentById_ReturnsDepartment_WhenDepartmentExists()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.GetDepartmentByIdAsync(1)).ReturnsAsync(new Department { Id = 1, DepartmentName = "IT" });
        var controller = CreateController(service);

        var result = await controller.GetDepartmentById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var department = Assert.IsType<Department>(okResult.Value);
        Assert.Equal("IT", department.DepartmentName);
    }

    [Fact]
    public async Task GetDepartmentById_ThrowsException_WhenDepartmentNotFound()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.GetDepartmentByIdAsync(999)).ReturnsAsync((Department?)null);
        var controller = CreateController(service);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => controller.GetDepartmentById(999));
    }

    [Fact]
    public async Task GetAllDepartments_ReturnsDepartments()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.GetAllDepartmentsAsync(It.IsAny<DepartmentSearchDto>())).ReturnsAsync([
            new Department { Id = 1, DepartmentName = "IT" },
            new Department { Id = 2, DepartmentName = "HR" }
        ]);
        var controller = CreateController(service);

        var result = await controller.GetAllDepartments(new DepartmentSearchDto { PageNumber = 1, PageSize = 10 });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var departments = Assert.IsAssignableFrom<List<Department>>(okResult.Value);
        Assert.Equal(2, departments.Count);
    }

    [Fact]
    public async Task AddDepartment_CreatesDepartment()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.AddDepartmentAsync(It.IsAny<DepartmentDto>(), It.IsAny<string>())).ReturnsAsync(new Department { Id = 1, DepartmentName = "Finance" });
        var controller = CreateController(service);
        SetUser(controller);

        var result = await controller.AddDepartment(new DepartmentDto { DepartmentName = "Finance" });

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var department = Assert.IsType<Department>(createdResult.Value);
        Assert.Equal("Finance", department.DepartmentName);
    }

    [Fact]
    public async Task AddDepartment_ThrowsException_WhenDepartmentAlreadyExists()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.AddDepartmentAsync(It.IsAny<DepartmentDto>(), It.IsAny<string>())).ThrowsAsync(new ArgumentException("Department already exists."));
        var controller = CreateController(service);

        await Assert.ThrowsAsync<ArgumentException>(() => controller.AddDepartment(new DepartmentDto { DepartmentName = "Finance" }));
    }

    [Fact]
    public async Task UpdateDepartment_UpdatesDepartment()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.UpdateDepartmentAsync(1, It.IsAny<DepartmentDto>(), It.IsAny<string>())).ReturnsAsync(new Department { Id = 1, DepartmentName = "Finance" });
        var controller = CreateController(service);
        SetUser(controller);

        var result = await controller.UpdateDepartment(1, new DepartmentDto { DepartmentName = "Finance" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var department = Assert.IsType<Department>(okResult.Value);
        Assert.Equal("Finance", department.DepartmentName);
    }

    [Fact]
    public async Task UpdateDepartment_ThrowsException_WhenDepartmentNotFound()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.UpdateDepartmentAsync(999, It.IsAny<DepartmentDto>(), It.IsAny<string>())).ReturnsAsync((Department?)null);
        var controller = CreateController(service);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => controller.UpdateDepartment(999, new DepartmentDto { DepartmentName = "Finance" }));
    }

    [Fact]
    public async Task UpdateDepartment_ThrowsException_WhenDepartmentNameAlreadyExists()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.UpdateDepartmentAsync(1, It.IsAny<DepartmentDto>(), It.IsAny<string>())).ThrowsAsync(new ArgumentException("Department already exists."));
        var controller = CreateController(service);

        await Assert.ThrowsAsync<ArgumentException>(() => controller.UpdateDepartment(1, new DepartmentDto { DepartmentName = "HR" }));
    }

    [Fact]
    public async Task DeleteDepartment_RemovesDepartment()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.DeleteDepartmentAsync(1, It.IsAny<string>())).ReturnsAsync(true);
        var controller = CreateController(service);
        SetUser(controller);

        var result = await controller.DeleteDepartment(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteDepartment_ThrowsException_WhenDepartmentNotFound()
    {
        var service = new Mock<IDepartmentService>();
        service.Setup(s => s.DeleteDepartmentAsync(999, It.IsAny<string>())).ReturnsAsync(false);
        var controller = CreateController(service);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => controller.DeleteDepartment(999));
    }
}