using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeAdminPortal.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService projectService;
        private readonly ILogger<ProjectsController> logger;

        public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
        {
            this.projectService = projectService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects([FromQuery] ProjectSearchDto searchDto)
        {
            logger.LogInformation("GetAllProjects called with Search={Search}, PageNumber={PageNumber}, PageSize={PageSize}", searchDto.Search, searchDto.PageNumber, searchDto.PageSize);
            var projects = await projectService.GetAllProjectsAsync(searchDto);
            logger.LogInformation("Retrieved {Count} projects", projects.Count);
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            logger.LogInformation("GetProjectById endpoint called with Id: {Id}", id);
            var project = await projectService.GetProjectByIdAsync(id);
            if (project is null)
            {
                logger.LogWarning("Project with Id: {Id} not found", id);
                throw new KeyNotFoundException("Project not found");
            }

            logger.LogInformation("Project with Id: {Id} retrieved successfully", id);
            return Ok(project);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddProject(ProjectDto dto)
        {
            logger.LogInformation("AddProject endpoint called with ProjectName: {ProjectName}", dto?.ProjectName);
            if (dto is null)
            {
                logger.LogWarning("AddProject endpoint called with null ProjectDto");
                throw new ArgumentException("Project data is required.");
            }

            var project = await projectService.AddProjectAsync(dto, User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown");
            logger.LogInformation("Project added successfully with Id: {Id}", project.Id);
            return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectDto dto)
        {
            logger.LogInformation("UpdateProject endpoint called with Id: {Id}, ProjectName: {ProjectName}", id, dto?.ProjectName);
            if (dto is null)
            {
                logger.LogWarning("UpdateProject endpoint called with null ProjectDto for Id: {Id}", id);
                throw new ArgumentException("Project data is required.");
            }

            var project = await projectService.UpdateProjectAsync(id, dto, User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown");
            if (project is null)
            {
                logger.LogWarning("Project with Id: {Id} not found", id);
                throw new KeyNotFoundException("Project not found");
            }

            logger.LogInformation("Project with Id: {Id} updated successfully", id);
            return Ok(project);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            logger.LogInformation("DeleteProject endpoint called with Id: {Id}", id);
            var deleted = await projectService.DeleteProjectAsync(id, User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown");
            if (!deleted)
            {
                logger.LogWarning("Project with Id: {Id} not found", id);
                throw new KeyNotFoundException("Project not found");
            }

            logger.LogInformation("Project with Id: {Id} deleted successfully", id);
            return NoContent();
        }
    }
}