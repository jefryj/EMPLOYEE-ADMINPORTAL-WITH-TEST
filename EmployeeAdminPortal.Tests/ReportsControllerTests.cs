using EmployeeAdminPortal.Controllers;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeAdminPortal.Tests.Controllers;

public class ReportsControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new ApplicationDbContext(options);
    }

    private ReportsController GetController(ApplicationDbContext dbContext)
    {
        var logger =
            Mock.Of<ILogger<ReportsController>>();

        var cache =
            new MemoryCache(new MemoryCacheOptions());

        return new ReportsController(
            dbContext,
            logger,
            cache);
    }

    [Fact]
    public async Task GetDepartmentSummary_ReturnsDepartmentSummary()
    {
        var dbContext = GetDbContext();

        await dbContext.Departments.AddAsync(new Department
        {
            Id = 1,
            DepartmentName = "IT"
        });

        await dbContext.Employees.AddRangeAsync(
            new Employee
            {
                Id = Guid.NewGuid(),
                Name = "John",
                Email = "john@test.com",
                DepartmentId = 1,
                Salary = 50000
            },
            new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Alex",
                Email = "alex@test.com",
                DepartmentId = 1,
                Salary = 60000
            });

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var result = await controller.GetDepartmentSummary();

        var okResult =
            Assert.IsType<OkObjectResult>(result);

        var report =
            Assert.IsAssignableFrom<IEnumerable<object>>(
                okResult.Value);

        var item = report.First();

        Assert.Equal(
            "IT",
            item.GetType()
                .GetProperty("DepartmentName")!
                .GetValue(item));

        Assert.Equal(
            2,
            item.GetType()
                .GetProperty("EmployeeCount")!
                .GetValue(item));

        Assert.Equal(
            55000m,
            item.GetType()
                .GetProperty("AverageSalary")!
                .GetValue(item));
    }

    [Fact]
    public async Task GetProjectSummary_ReturnsProjectSummary()
    {
        var dbContext = GetDbContext();

        await dbContext.Projects.AddAsync(new Project
        {
            Id = 1,
            ProjectName = "Employee Portal",
            ProjectMembersCount = 2
        });

        await dbContext.Employees.AddRangeAsync(
            new Employee
            {
                Id = Guid.NewGuid(),
                Name = "John",
                Email = "john@test.com",
                ProjectId = 1,
                Salary = 50000
            },
            new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Alex",
                Email = "alex@test.com",
                ProjectId = 1,
                Salary = 70000
            });

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var result = await controller.GetProjectSummary();

        var okResult =
            Assert.IsType<OkObjectResult>(result);

        var report =
            Assert.IsAssignableFrom<IEnumerable<object>>(
                okResult.Value);

        var item = report.First();

        Assert.Equal(
            "Employee Portal",
            item.GetType()
                .GetProperty("ProjectName")!
                .GetValue(item));

        Assert.Equal(
            2,
            item.GetType()
                .GetProperty("EmployeeCount")!
                .GetValue(item));

        Assert.Equal(
            60000m,
            item.GetType()
                .GetProperty("AverageSalary")!
                .GetValue(item));
    }

    [Fact]
    public async Task GetDepartmentSummary_ReturnsZeroForDepartmentWithNoEmployees()
    {
        var dbContext = GetDbContext();

        await dbContext.Departments.AddAsync(new Department
        {
            Id = 1,
            DepartmentName = "Finance"
        });

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var result = await controller.GetDepartmentSummary();

        var okResult =
            Assert.IsType<OkObjectResult>(result);

        var report =
            Assert.IsAssignableFrom<IEnumerable<object>>(
                okResult.Value);

        var item = report.First();

        Assert.Equal(
            0,
            item.GetType()
                .GetProperty("EmployeeCount")!
                .GetValue(item));

        Assert.Equal(
            0m,
            item.GetType()
                .GetProperty("AverageSalary")!
                .GetValue(item));
    }
}
