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

public class ProjectServiceTests
{
    private static ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new ApplicationDbContext(options);
    }

    private static ProjectService CreateService(ApplicationDbContext dbContext, IMemoryCache? cache = null)
    {
        return new ProjectService(
            new ProjectRepository(dbContext),
            new AuditLogRepository(dbContext),
            cache ?? new MemoryCache(new MemoryCacheOptions()),
            Mock.Of<ILogger<ProjectService>>());
    }

    [Fact]
    public async Task GetProjectByIdAsync_ReturnsProject_WhenProjectExists()
    {
        var dbContext = GetDbContext();
        await dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "Employee Portal", ProjectMembersCount = 5 });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.GetProjectByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Employee Portal", result.ProjectName);
    }

    [Fact]
    public async Task GetAllProjectsAsync_ReturnsProjects()
    {
        var dbContext = GetDbContext();
        await dbContext.Projects.AddRangeAsync(
            new Project { Id = 1, ProjectName = "Employee Portal", ProjectMembersCount = 5 },
            new Project { Id = 2, ProjectName = "HR System", ProjectMembersCount = 3 });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.GetAllProjectsAsync(new ProjectSearchDto { PageNumber = 1, PageSize = 10 });

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllProjectsAsync_ReturnsMatchingProjects_WhenSearchProvided()
    {
        var dbContext = GetDbContext();
        await dbContext.Projects.AddRangeAsync(
            new Project { Id = 1, ProjectName = "Employee Portal", ProjectMembersCount = 5 },
            new Project { Id = 2, ProjectName = "HR System", ProjectMembersCount = 3 });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.GetAllProjectsAsync(new ProjectSearchDto { Search = "Employee", PageNumber = 1, PageSize = 10 });

        Assert.Single(result);
        Assert.Equal("Employee Portal", result[0].ProjectName);
    }

    [Fact]
    public async Task AddProjectAsync_CreatesProject()
    {
        var dbContext = GetDbContext();
        var service = CreateService(dbContext);

        var result = await service.AddProjectAsync(new ProjectDto { ProjectName = "New Project" }, "admin@test.com");

        Assert.Equal("New Project", result.ProjectName);
        Assert.Equal(0, result.ProjectMembersCount);
        Assert.Single(dbContext.Projects);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task AddProjectAsync_ThrowsException_WhenProjectAlreadyExists()
    {
        var dbContext = GetDbContext();
        await dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "Employee Portal", ProjectMembersCount = 5 });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        await Assert.ThrowsAsync<ArgumentException>(() => service.AddProjectAsync(new ProjectDto { ProjectName = "Employee Portal" }, "admin@test.com"));
    }

    [Fact]
    public async Task UpdateProjectAsync_UpdatesProject()
    {
        var dbContext = GetDbContext();
        await dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "Old Project", ProjectMembersCount = 5 });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.UpdateProjectAsync(1, new ProjectDto { ProjectName = "Updated Project" }, "admin@test.com");

        Assert.NotNull(result);
        Assert.Equal("Updated Project", result.ProjectName);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task UpdateProjectAsync_ReturnsNull_WhenProjectNotFound()
    {
        var service = CreateService(GetDbContext());

        var result = await service.UpdateProjectAsync(999, new ProjectDto { ProjectName = "Updated Project" }, "admin@test.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProjectAsync_ThrowsException_WhenProjectNameAlreadyExists()
    {
        var dbContext = GetDbContext();
        await dbContext.Projects.AddRangeAsync(
            new Project { Id = 1, ProjectName = "Project One", ProjectMembersCount = 2 },
            new Project { Id = 2, ProjectName = "Project Two", ProjectMembersCount = 3 });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateProjectAsync(1, new ProjectDto { ProjectName = "Project Two" }, "admin@test.com"));
    }

    [Fact]
    public async Task DeleteProjectAsync_DeletesProject()
    {
        var dbContext = GetDbContext();
        await dbContext.Projects.AddAsync(new Project { Id = 1, ProjectName = "Project One", ProjectMembersCount = 2 });
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.DeleteProjectAsync(1, "admin@test.com");

        Assert.True(result);
        Assert.Empty(dbContext.Projects);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task DeleteProjectAsync_ReturnsFalse_WhenProjectNotFound()
    {
        var service = CreateService(GetDbContext());

        var result = await service.DeleteProjectAsync(999, "admin@test.com");

        Assert.False(result);
    }
}