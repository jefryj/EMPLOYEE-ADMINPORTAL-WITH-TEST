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

public class ProjectsControllerTests
{
    private static ProjectsController CreateController(Mock<IProjectService> service)
    {
        return new ProjectsController(service.Object, Mock.Of<ILogger<ProjectsController>>());
    }

    private static void SetUser(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "admin@test.com") }, "TestAuthentication"))
            }
        };
    }

    [Fact]
    public async Task GetProjectById_ReturnsProject_WhenProjectExists()
    {
        var service = new Mock<IProjectService>();
        service.Setup(s => s.GetProjectByIdAsync(1)).ReturnsAsync(new Project { Id = 1, ProjectName = "Employee Portal", ProjectMembersCount = 5 });
        var controller = CreateController(service);

        var result = await controller.GetProjectById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var project = Assert.IsType<Project>(okResult.Value);
        Assert.Equal("Employee Portal", project.ProjectName);
    }

    [Fact]
    public async Task GetProjectById_ThrowsException_WhenProjectNotFound()
    {
        var service = new Mock<IProjectService>();
        service.Setup(s => s.GetProjectByIdAsync(999)).ReturnsAsync((Project?)null);
        var controller = CreateController(service);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => controller.GetProjectById(999));
    }

    [Fact]
    public async Task GetAllProjects_ReturnsProjects()
    {
        var service = new Mock<IProjectService>();
        service.Setup(s => s.GetAllProjectsAsync(It.IsAny<ProjectSearchDto>())).ReturnsAsync([
            new Project { Id = 1, ProjectName = "Employee Portal", ProjectMembersCount = 5 },
            new Project { Id = 2, ProjectName = "HR System", ProjectMembersCount = 3 }
        ]);
        var controller = CreateController(service);

        var result = await controller.GetAllProjects(new ProjectSearchDto { PageNumber = 1, PageSize = 10 });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var projects = Assert.IsAssignableFrom<List<Project>>(okResult.Value);
        Assert.Equal(2, projects.Count);
    }

    [Fact]
    public async Task GetAllProjects_ReturnsMatchingProjects_WhenSearchProvided()
    {
        var service = new Mock<IProjectService>();
        service.Setup(s => s.GetAllProjectsAsync(It.Is<ProjectSearchDto>(dto => dto.Search == "Employee"))).ReturnsAsync([
            new Project { Id = 1, ProjectName = "Employee Portal", ProjectMembersCount = 5 }
        ]);
        var controller = CreateController(service);

        var result = await controller.GetAllProjects(new ProjectSearchDto { Search = "Employee", PageNumber = 1, PageSize = 10 });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var projects = Assert.IsAssignableFrom<List<Project>>(okResult.Value);
        Assert.Single(projects);
        Assert.Equal("Employee Portal", projects[0].ProjectName);
    }

    [Fact]
    public async Task AddProject_CreatesProject()
    {
        var service = new Mock<IProjectService>();
        service.Setup(s => s.AddProjectAsync(It.IsAny<ProjectDto>(), It.IsAny<string>())).ReturnsAsync(new Project { Id = 1, ProjectName = "New Project", ProjectMembersCount = 0 });
        var controller = CreateController(service);
        SetUser(controller);

        var result = await controller.AddProject(new ProjectDto { ProjectName = "New Project" });

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var project = Assert.IsType<Project>(createdResult.Value);
        Assert.Equal("New Project", project.ProjectName);
        Assert.Equal(0, project.ProjectMembersCount);
    }

    [Fact]
    public async Task AddProject_ThrowsException_WhenProjectAlreadyExists()
    {
        var service = new Mock<IProjectService>();
        service.Setup(s => s.AddProjectAsync(It.IsAny<ProjectDto>(), It.IsAny<string>())).ThrowsAsync(new ArgumentException("Project already exists."));
        var controller = CreateController(service);

        await Assert.ThrowsAsync<ArgumentException>(() => controller.AddProject(new ProjectDto { ProjectName = "Employee Portal" }));
    }

    [Fact]
    public async Task AddProject_ThrowsException_WhenDtoIsNull()
    {
        var service = new Mock<IProjectService>();
        var controller = CreateController(service);

        await Assert.ThrowsAsync<ArgumentException>(() => controller.AddProject(null!));
    }

    [Fact]
    public async Task UpdateProject_UpdatesProject()
    {
        var service = new Mock<IProjectService>();
        service.Setup(s => s.UpdateProjectAsync(1, It.IsAny<ProjectDto>(), It.IsAny<string>())).ReturnsAsync(new Project { Id = 1, ProjectName = "Updated Project", ProjectMembersCount = 5 });
        var controller = CreateController(service);
        SetUser(controller);

        var result = await controller.UpdateProject(1, new ProjectDto { ProjectName = "Updated Project" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var project = Assert.IsType<Project>(okResult.Value);
        Assert.Equal("Updated Project", project.ProjectName);
    }

    [Fact]
    public async Task UpdateProject_ThrowsException_WhenProjectNotFound()
    {
        var service = new Mock<IProjectService>();
        service.Setup(s => s.UpdateProjectAsync(999, It.IsAny<ProjectDto>(), It.IsAny<string>())).ReturnsAsync((Project?)null);
        var controller = CreateController(service);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => controller.UpdateProject(999, new ProjectDto { ProjectName = "Updated Project" }));
    }

    [Fact]
    public async Task UpdateProject_ThrowsException_WhenProjectNameAlreadyExists()
    {
        var service = new Mock<IProjectService>();
        service.Setup(s => s.UpdateProjectAsync(1, It.IsAny<ProjectDto>(), It.IsAny<string>())).ThrowsAsync(new ArgumentException("Project already exists."));
        var controller = CreateController(service);

        await Assert.ThrowsAsync<ArgumentException>(() => controller.UpdateProject(1, new ProjectDto { ProjectName = "Project Two" }));
    }

    [Fact]
    public async Task UpdateProject_ThrowsException_WhenDtoIsNull()
    {
        var service = new Mock<IProjectService>();
        var controller = CreateController(service);

        await Assert.ThrowsAsync<ArgumentException>(() => controller.UpdateProject(1, null!));
    }

    [Fact]
    public async Task DeleteProject_DeletesProject()
    {
        var service = new Mock<IProjectService>();
        service.Setup(s => s.DeleteProjectAsync(1, It.IsAny<string>())).ReturnsAsync(true);
        var controller = CreateController(service);
        SetUser(controller);

        var result = await controller.DeleteProject(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteProject_ThrowsException_WhenProjectNotFound()
    {
        var service = new Mock<IProjectService>();
        service.Setup(s => s.DeleteProjectAsync(999, It.IsAny<string>())).ReturnsAsync(false);
        var controller = CreateController(service);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => controller.DeleteProject(999));
    }
}