using EmployeeAdminPortal.Controllers;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace EmployeeAdminPortal.Tests.Controllers;

public class ProjectsControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        return new ApplicationDbContext(options);
    }

    private ProjectsController GetController(ApplicationDbContext dbContext)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());

        var logger = Mock.Of<ILogger<ProjectsController>>();

        return new ProjectsController(dbContext, cache, logger);
    }

    [Fact]
    public async Task GetProjectById_ReturnsProject_WhenProjectExists()
    {
        var dbContext = GetDbContext();

        await dbContext.Projects.AddAsync(new Project
        {
            Id = 1,
            ProjectName = "Employee Portal",
            ProjectMembersCount = 5
        });

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var result = await controller.GetProjectById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);

        var project = Assert.IsType<Project>(okResult.Value);

        Assert.Equal("Employee Portal",project.ProjectName);
    }

    [Fact]
    public async Task GetProjectById_ThrowsException_WhenProjectNotFound()
    {
        var dbContext = GetDbContext();

        var controller = GetController(dbContext);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => controller.GetProjectById(999));
    }

    [Fact]
    public async Task GetAllProjects_ReturnsProjects()
    {
        var dbContext = GetDbContext();

        await dbContext.Projects.AddRangeAsync(
            new Project
            {
                Id = 1,
                ProjectName = "Employee Portal",
                ProjectMembersCount = 5
            },
            new Project
            {
                Id = 2,
                ProjectName = "HR System",
                ProjectMembersCount = 3
            });

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var searchDto = new ProjectSearchDto
        {
            PageNumber = 1,
            PageSize = 10
        };

        var result = await controller.GetAllProjects(searchDto);

        var okResult = Assert.IsType<OkObjectResult>(result);

        var projects = Assert.IsAssignableFrom<List<Project>>(okResult.Value);

        Assert.Equal(2, projects.Count);
    }

    [Fact]
    public async Task GetAllProjects_ReturnsMatchingProjects_WhenSearchProvided()
    {
        var dbContext = GetDbContext();

        await dbContext.Projects.AddRangeAsync(
            new Project
            {
                Id = 1,
                ProjectName = "Employee Portal",
                ProjectMembersCount = 5
            },
            new Project
            {
                Id = 2,
                ProjectName = "HR System",
                ProjectMembersCount = 3
            });

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var searchDto = new ProjectSearchDto
        {
            Search = "Employee",
            PageNumber = 1,
            PageSize = 10
        };

        var result = await controller.GetAllProjects(searchDto);

        var okResult = Assert.IsType<OkObjectResult>(result);

        var projects = Assert.IsAssignableFrom<List<Project>>(okResult.Value);

        Assert.Single(projects);
        Assert.Equal(
            "Employee Portal",
            projects[0].ProjectName);
    }

    [Fact]
    public async Task AddProject_CreatesProject()
    {
        var dbContext = GetDbContext();

        var controller = GetController(dbContext);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[]
                {
                    new Claim(
                        ClaimTypes.Email,
                        "admin@test.com")
                },
                "TestAuthentication"));

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User = user
                    }
            };

        var dto = new ProjectDto
        {
            ProjectName = "New Project"
        };

        var result = await controller.AddProject(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);

        var project = Assert.IsType<Project>(createdResult.Value);

        Assert.Equal("New Project",project.ProjectName);

        Assert.Equal(0,project.ProjectMembersCount);

        Assert.Single(dbContext.Projects);

        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task AddProject_ThrowsException_WhenProjectAlreadyExists()
    {
        var dbContext = GetDbContext();

        await dbContext.Projects.AddAsync(new Project
        {
            Id = 1,
            ProjectName = "Employee Portal",
            ProjectMembersCount = 5
        });

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var dto = new ProjectDto
        {
            ProjectName = "Employee Portal"
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => controller.AddProject(dto));
    }

    [Fact]
    public async Task AddProject_ThrowsException_WhenDtoIsNull()
    {
        var dbContext = GetDbContext();

        var controller = GetController(dbContext);

        await Assert.ThrowsAsync<ArgumentException>(
            () => controller.AddProject(null!));
    }

    [Fact]
    public async Task UpdateProject_UpdatesProject()
    {
        var dbContext = GetDbContext();

        await dbContext.Projects.AddAsync(new Project
        {
            Id = 1,
            ProjectName = "Old Project",
            ProjectMembersCount = 5
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
                "TestAuthentication"));

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User = user
                    }
            };

        var dto = new ProjectDto
        {
            ProjectName = "Updated Project"
        };

        var result = await controller.UpdateProject(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);

        var project = Assert.IsType<Project>(okResult.Value);

        Assert.Equal("Updated Project",project.ProjectName);

        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task UpdateProject_ThrowsException_WhenProjectNotFound()
    {
        var dbContext = GetDbContext();

        var controller = GetController(dbContext);

        var dto = new ProjectDto
        {
            ProjectName = "Updated Project"
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => controller.UpdateProject(999, dto));
    }

    [Fact]
    public async Task UpdateProject_ThrowsException_WhenProjectNameAlreadyExists()
    {
        var dbContext = GetDbContext();

        await dbContext.Projects.AddRangeAsync(
            new Project
            {
                Id = 1,
                ProjectName = "Project One",
                ProjectMembersCount = 2
            },
            new Project
            {
                Id = 2,
                ProjectName = "Project Two",
                ProjectMembersCount = 3
            });

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var dto = new ProjectDto
        {
            ProjectName = "Project Two"
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => controller.UpdateProject(1, dto));
    }

    [Fact]
    public async Task UpdateProject_ThrowsException_WhenDtoIsNull()
    {
        var dbContext = GetDbContext();

        await dbContext.Projects.AddAsync(new Project
        {
            Id = 1,
            ProjectName = "Project One",
            ProjectMembersCount = 2
        });

        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        await Assert.ThrowsAsync<ArgumentException>(
            () => controller.UpdateProject(1, null!));
    }

    [Fact]
    public async Task DeleteProject_DeletesProject()
    {
        var dbContext = GetDbContext();

        await dbContext.Projects.AddAsync(new Project
        {
            Id = 1,
            ProjectName = "Project One",
            ProjectMembersCount = 2
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
                "TestAuthentication"));

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User = user
                    }
            };

        var result = await controller.DeleteProject(1);

        Assert.IsType<NoContentResult>(result);

        Assert.Empty(dbContext.Projects);

        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public async Task DeleteProject_ThrowsException_WhenProjectNotFound()
    {
        var dbContext = GetDbContext();

        var controller = GetController(dbContext);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => controller.DeleteProject(999));
    }
}
