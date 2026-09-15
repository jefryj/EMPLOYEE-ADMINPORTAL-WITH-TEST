using EmployeeAdminPortal.Controllers;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EmployeeAdminPortal.Tests.Controllers;

public class DepartmentsControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetDepartmentById_ReturnsDepartment_WhenDepartmentExists()
    {
        // Arrange
        var dbContext = GetDbContext();

        dbContext.Departments.Add(new Department
        {
            Id = 1,
            DepartmentName = "IT"
        });

        await dbContext.SaveChangesAsync();

        var cache = new MemoryCache(new MemoryCacheOptions());

        var controller = new DepartmentsController(dbContext, cache);
        
        var result = await controller.GetDepartmentById(1);

        
        var okResult = Assert.IsType<OkObjectResult>(result);

        var department = Assert.IsType<Department>(okResult.Value);

        Assert.Equal("IT", department.DepartmentName);
    }

    [Fact]
    public async Task GetDepartmentById_ThrowsException_WhenDepartmentNotFound()
    {
        var dbContext = GetDbContext();
        var cache = new MemoryCache(new MemoryCacheOptions());

        var controller = new DepartmentsController(dbContext, cache);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => controller.GetDepartmentById(999));
    }

    [Fact]
    public async Task GetAllDepartments_ReturnsDepartments()
    {
        // Arrange
        var dbContext = GetDbContext();

        dbContext.Departments.AddRange(
            new Department
            {
                Id = 1,
                DepartmentName = "IT"
            },
            new Department
            {
                Id = 2,
                DepartmentName = "HR"
            });

        await dbContext.SaveChangesAsync();

        var cache = new MemoryCache(new MemoryCacheOptions());

        var controller = new DepartmentsController(dbContext, cache);

        var searchDto = new DepartmentSearchDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await controller.GetAllDepartments(searchDto);

        var okResult = Assert.IsType<OkObjectResult>(result);

        var departments =
            Assert.IsAssignableFrom<List<Department>>(
                okResult.Value);

        Assert.Equal(2, departments.Count);
    }

[Fact]
public async Task AddDepartment_CreatesDepartment()
{
    var dbContext = GetDbContext();

    var cache = new MemoryCache(new MemoryCacheOptions());

    var controller = new DepartmentsController(dbContext,cache);

    var user = new ClaimsPrincipal(
        new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Email,"test@test.com")
            },
            "TestAuthentication"));

    controller.ControllerContext = new ControllerContext
        {
            HttpContext =new DefaultHttpContext
                {
                    User = user
                }
        };

    var dto = new DepartmentDto
    {
        DepartmentName = "Finance"
    };

    
    var result =
        await controller.AddDepartment(dto);

    var createdResult = Assert.IsType<CreatedAtActionResult>(result);

    Assert.Equal(1,dbContext.Departments.Count());

    Assert.Single(dbContext.AuditLogs);
}
[Fact]
public async Task AddDepartment_ThrowsException_WhenDepartmentAlreadyExists()
{
    var dbContext = GetDbContext();

    dbContext.Departments.Add(new Department
    {
    DepartmentName = "Finance"
    });

    await dbContext.SaveChangesAsync();

    var cache = new MemoryCache(new MemoryCacheOptions());

    var controller = new DepartmentsController(dbContext, cache);

    var dto = new DepartmentDto
    {
    DepartmentName = "Finance"
    };

    await Assert.ThrowsAsync<ArgumentException>(
    () => controller.AddDepartment(dto));
}

[Fact]
public async Task UpdateDepartment_UpdatesDepartment()
{
    var dbContext = GetDbContext();

    dbContext.Departments.Add(new Department
    {
    Id = 1,
    DepartmentName = "IT"
    });

    await dbContext.SaveChangesAsync();

    var cache = new MemoryCache(new MemoryCacheOptions());

    var controller = new DepartmentsController(dbContext, cache);

    var user = new ClaimsPrincipal(
    new ClaimsIdentity(
    new[]
    {
    new Claim(ClaimTypes.Email,"test@test.com")
    },"TestAuthentication"));

    controller.ControllerContext = new ControllerContext
    {
    HttpContext = new DefaultHttpContext
    {
    User = user
    }
    };

    var dto = new DepartmentDto
    {
    DepartmentName = "Finance"
    };

    var result = await controller.UpdateDepartment(1, dto);

    var okResult = Assert.IsType<OkObjectResult>(result);

    var department = Assert.IsType<Department>(okResult.Value);

    Assert.Equal("Finance", department.DepartmentName);

    Assert.Single(dbContext.AuditLogs);
}

[Fact]
public async Task UpdateDepartment_ThrowsException_WhenDepartmentNotFound()
{
    var dbContext = GetDbContext();

    var cache = new MemoryCache(new MemoryCacheOptions());

    var controller = new DepartmentsController(dbContext, cache);

    var dto = new DepartmentDto
    {
    DepartmentName = "Finance"
    };

    await Assert.ThrowsAsync<KeyNotFoundException>(
    () => controller.UpdateDepartment(999, dto));
    }

    [Fact]
    public async Task UpdateDepartment_ThrowsException_WhenDepartmentNameAlreadyExists()
    {
    var dbContext = GetDbContext();

    dbContext.Departments.AddRange(
    new Department
    {
    Id = 1,
    DepartmentName = "IT"
    },
    new Department
    {
    Id = 2,
    DepartmentName = "HR"
    });

    await dbContext.SaveChangesAsync();

    var cache = new MemoryCache(new MemoryCacheOptions());

    var controller = new DepartmentsController(dbContext, cache);

    var dto = new DepartmentDto
    {
    DepartmentName = "HR"
    };

    await Assert.ThrowsAsync<ArgumentException>(
    () => controller.UpdateDepartment(1, dto));
}

[Fact]
public async Task DeleteDepartment_RemovesDepartment()
{
    var dbContext = GetDbContext();

    dbContext.Departments.Add(new Department
    {
    Id = 1,
    DepartmentName = "IT"
    });

    await dbContext.SaveChangesAsync();

    var cache = new MemoryCache(new MemoryCacheOptions());

    var controller = new DepartmentsController(dbContext, cache);

    var user = new ClaimsPrincipal(
    new ClaimsIdentity(
    new[]
    {
    new Claim(ClaimTypes.Email,"test@test.com")
    },
    "TestAuthentication"));

    controller.ControllerContext = new ControllerContext
    {
    HttpContext = new DefaultHttpContext
    {
        User = user
    }
    };

    var result = await controller.DeleteDepartment(1);

    Assert.IsType<NoContentResult>(result);

    Assert.Empty(dbContext.Departments);

    Assert.Single(dbContext.AuditLogs);
}

[Fact]
public async Task DeleteDepartment_ThrowsException_WhenDepartmentNotFound()
    {
    var dbContext = GetDbContext();

    var cache = new MemoryCache(new MemoryCacheOptions());

    var controller = new DepartmentsController(dbContext, cache);

    await Assert.ThrowsAsync<KeyNotFoundException>(
    () => controller.DeleteDepartment(999));
}
}