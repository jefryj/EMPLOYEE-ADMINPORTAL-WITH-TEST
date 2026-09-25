using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories;
using EmployeeAdminPortal.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeAdminPortal.Tests;

public class DepartmentServiceTests
{
    private static ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new ApplicationDbContext(options);
    }

    private static DepartmentService CreateService(ApplicationDbContext dbContext, IMemoryCache? cache = null)
    {
        return new DepartmentService(
            new DepartmentRepository(dbContext),
            new AuditLogRepository(dbContext),
            cache ?? new MemoryCache(new MemoryCacheOptions()),
            Mock.Of<ILogger<DepartmentService>>());
    }

    [Fact]
    public async Task GetDepartmentByIdAsync_ReturnsDepartment_WhenDepartmentExists()
    {
        var dbContext = GetDbContext();
        dbContext.Departments.Add(new Department { Id = 1, DepartmentName = "IT" });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.GetDepartmentByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("IT", result.DepartmentName);
    }

    [Fact]
    public async Task GetAllDepartmentsAsync_ReturnsDepartments()
    {
        var dbContext = GetDbContext();
        dbContext.Departments.AddRange(new Department { Id = 1, DepartmentName = "IT" }, new Department { Id = 2, DepartmentName = "HR" });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.GetAllDepartmentsAsync(new DepartmentSearchDto { PageNumber = 1, PageSize = 10 });

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task AddDepartmentAsync_CreatesDepartment()
    {
        var dbContext = GetDbContext();
        var service = CreateService(dbContext);

        var result = await service.AddDepartmentAsync(new DepartmentDto { DepartmentName = "Finance" }, "test@test.com");

        Assert.Equal("Finance", result.DepartmentName);
        Assert.Single(dbContext.Departments);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task AddDepartmentAsync_ThrowsException_WhenDepartmentAlreadyExists()
    {
        var dbContext = GetDbContext();
        dbContext.Departments.Add(new Department { DepartmentName = "Finance" });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        await Assert.ThrowsAsync<ArgumentException>(() => service.AddDepartmentAsync(new DepartmentDto { DepartmentName = "Finance" }, "test@test.com"));
    }

    [Fact]
    public async Task UpdateDepartmentAsync_UpdatesDepartment()
    {
        var dbContext = GetDbContext();
        dbContext.Departments.Add(new Department { Id = 1, DepartmentName = "IT" });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.UpdateDepartmentAsync(1, new DepartmentDto { DepartmentName = "Finance" }, "test@test.com");

        Assert.NotNull(result);
        Assert.Equal("Finance", result.DepartmentName);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_ReturnsNull_WhenDepartmentNotFound()
    {
        var service = CreateService(GetDbContext());

        var result = await service.UpdateDepartmentAsync(999, new DepartmentDto { DepartmentName = "Finance" }, "test@test.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateDepartmentAsync_ThrowsException_WhenDepartmentNameAlreadyExists()
    {
        var dbContext = GetDbContext();
        dbContext.Departments.AddRange(new Department { Id = 1, DepartmentName = "IT" }, new Department { Id = 2, DepartmentName = "HR" });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateDepartmentAsync(1, new DepartmentDto { DepartmentName = "HR" }, "test@test.com"));
    }

    [Fact]
    public async Task DeleteDepartmentAsync_RemovesDepartment()
    {
        var dbContext = GetDbContext();
        dbContext.Departments.Add(new Department { Id = 1, DepartmentName = "IT" });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.DeleteDepartmentAsync(1, "test@test.com");

        Assert.True(result);
        Assert.Empty(dbContext.Departments);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task DeleteDepartmentAsync_ReturnsFalse_WhenDepartmentNotFound()
    {
        var service = CreateService(GetDbContext());

        var result = await service.DeleteDepartmentAsync(999, "test@test.com");

        Assert.False(result);
    }
}