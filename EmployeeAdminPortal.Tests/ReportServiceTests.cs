using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories;
using EmployeeAdminPortal.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeAdminPortal.Tests;

public class ReportServiceTests
{
    private static ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new ApplicationDbContext(options);
    }

    private static ReportService CreateService(ApplicationDbContext dbContext, IMemoryCache? cache = null)
    {
        return new ReportService(new ReportRepository(dbContext), cache ?? new MemoryCache(new MemoryCacheOptions()), Mock.Of<ILogger<ReportService>>());
    }

    [Fact]
    public async Task GetDepartmentSummaryAsync_ReturnsDepartmentSummary()
    {
        var dbContext = GetDbContext();
        await dbContext.Departments.AddAsync(new Department { Id = 1, DepartmentName = "IT" });
        await dbContext.Employees.AddRangeAsync(
            new Employee { Id = Guid.NewGuid(), Name = "John", Email = "john@test.com", DepartmentId = 1, Salary = 50000 },
            new Employee { Id = Guid.NewGuid(), Name = "Alex", Email = "alex@test.com", DepartmentId = 1, Salary = 60000 });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var report = await service.GetDepartmentSummaryAsync();

        var item = report.First();
        Assert.Equal("IT", item.DepartmentName);
        Assert.Equal(2, item.EmployeeCount);
        Assert.Equal(55000m, item.AverageSalary);
        Assert.Equal(50000m, item.MinSalary);
        Assert.Equal(60000m, item.MaxSalary);
    }

    [Fact]
    public async Task GetProjectSummaryAsync_ReturnsProjectSummary()
    {
        var dbContext = GetDbContext();
        await dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "Employee Portal", ProjectMembersCount = 2 });
        await dbContext.Employees.AddRangeAsync(
            new Employee { Id = Guid.NewGuid(), Name = "John", Email = "john@test.com", ProjectId = 1, Salary = 50000 },
            new Employee { Id = Guid.NewGuid(), Name = "Alex", Email = "alex@test.com", ProjectId = 1, Salary = 70000 });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var report = await service.GetProjectSummaryAsync();

        var item = report.First();
        Assert.Equal("Employee Portal", item.ProjectName);
        Assert.Equal(2, item.ProjectMembersCount);
        Assert.Equal(60000m, item.AverageSalary);
        Assert.Equal(50000m, item.MinSalary);
        Assert.Equal(70000m, item.MaxSalary);
    }

    [Fact]
    public async Task GetDepartmentSummaryAsync_ReturnsZeroForDepartmentWithNoEmployees()
    {
        var dbContext = GetDbContext();
        await dbContext.Departments.AddAsync(new Department { Id = 1, DepartmentName = "Finance" });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var report = await service.GetDepartmentSummaryAsync();

        var item = report.First();
        Assert.Equal(0, item.EmployeeCount);
        Assert.Equal(0m, item.AverageSalary);
        Assert.Equal(0m, item.MinSalary);
        Assert.Equal(0m, item.MaxSalary);
    }
}