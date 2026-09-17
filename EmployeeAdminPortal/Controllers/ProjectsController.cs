using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace EmployeeAdminPortal.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private static readonly List<string> cacheKeys = new();

        private readonly IMemoryCache cache;
        private readonly ApplicationDbContext dBcontext;
        private readonly ILogger<ProjectsController> logger;

        public ProjectsController(ApplicationDbContext dBcontext, IMemoryCache cache, ILogger<ProjectsController> logger)
        {
            this.dBcontext = dBcontext;
            this.cache = cache;
            this.logger = logger;
        }
        private void ClearProjectCache()
        {
            logger.LogInformation("Clearing project cache with {Count} entries", cacheKeys.Count);
            foreach (var key in cacheKeys)
            {
                cache.Remove(key);
            }
            cacheKeys.Clear();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProjects([FromQuery] ProjectSearchDto searchDto)
        {
            logger.LogInformation("GetAllProjects called with Search={Search}, PageNumber={PageNumber}, PageSize={PageSize}", searchDto.Search, searchDto.PageNumber, searchDto.PageSize);
            IQueryable<Project> query = dBcontext.Projects;

            if (!string.IsNullOrWhiteSpace(searchDto.Search))
            {
                logger.LogInformation("Applying project search filter: {Search}", searchDto.Search);
                query = query.Where(p => p.ProjectName.Contains(searchDto.Search));
            }

            string cacheKey = $"projects-{searchDto.Search}-{searchDto.PageNumber}-{searchDto.PageSize}";

            List<Project>? projects;

            bool foundInCache = cache.TryGetValue(cacheKey, out projects);

            if (!foundInCache)
            {
                logger.LogInformation("Project list cache miss for key {CacheKey}", cacheKey);
                projects = await query.OrderBy(p => p.Id).Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize).ToListAsync();

                cache.Set(cacheKey, projects, TimeSpan.FromMinutes(5));

                if (!cacheKeys.Contains(cacheKey))
                {
                    cacheKeys.Add(cacheKey);
                }
            }
            else
            {
                logger.LogInformation("Project list cache hit for key {CacheKey}", cacheKey);
            }

            logger.LogInformation("Retrieved {Count} projects", projects?.Count ?? 0);
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            logger.LogInformation("GetProjectById endpoint called with Id: {Id}", id);
            var project = await dBcontext.Projects.FindAsync(id);

            if (project == null)
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
            if(dto == null)
            {
                logger.LogWarning("AddProject endpoint called with null ProjectDto");
                throw new ArgumentException("Project data is required.");
            }
            bool projectExists = await dBcontext.Projects.AnyAsync(p => p.ProjectName == dto.ProjectName);

            if (projectExists)
            {
                logger.LogWarning("Project with name {ProjectName} already exists", dto.ProjectName);
                throw new ArgumentException("Project already exists.");
            }
            var project = new Project
            {
                
                ProjectName = dto.ProjectName,
                ProjectMembersCount = 0
            };

            await dBcontext.Projects.AddAsync(project);

            await dBcontext.SaveChangesAsync();

            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Create",
                EntityName = "Project",
                Details = $"Project {project.ProjectName} was created",
                CreatedAt = DateTime.UtcNow
            };

            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();
            ClearProjectCache();
            logger.LogInformation("Project added successfully with Id: {Id}", project.Id);

            return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectDto dto)
        {   
            logger.LogInformation("UpdateProject endpoint called with Id: {Id}, ProjectName: {ProjectName}", id, dto?.ProjectName);
            if(dto == null)
            {
                logger.LogWarning("UpdateProject endpoint called with null ProjectDto for Id: {Id}", id);
                throw new ArgumentException("Project data is required.");
            }
            var project = await dBcontext.Projects.FindAsync(id);

            if (project == null)
            {
                logger.LogWarning("Project with Id: {Id} not found", id);
                throw new KeyNotFoundException("Project not found");
            }
            bool projectExists = await dBcontext.Projects.AnyAsync(p => p.Id != id && p.ProjectName == dto.ProjectName);

            if (projectExists)
            {
                logger.LogWarning("Project with name {ProjectName} already exists", dto.ProjectName);
                throw new ArgumentException("Project already exists.");
            }

            project.ProjectName = dto.ProjectName;

            await dBcontext.SaveChangesAsync();
            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Update",
                EntityName = "Project",
                Details = $"Project {project.ProjectName} was updated",
                CreatedAt = DateTime.UtcNow
            };
            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();
            ClearProjectCache();
            logger.LogInformation("Project with Id: {Id} updated successfully", id);

            return Ok(project);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            logger.LogInformation("DeleteProject endpoint called with Id: {Id}", id);
            var project = await dBcontext.Projects.FindAsync(id);

            if (project == null)
            {
                logger.LogWarning("Project with Id: {Id} not found", id);
                throw new KeyNotFoundException("Project not found");
            }

            dBcontext.Projects.Remove(project);

            await dBcontext.SaveChangesAsync();
            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Delete",
                EntityName = "Project",
                Details = $"Project {project.ProjectName} was deleted",
                CreatedAt = DateTime.UtcNow
            };
            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();

            ClearProjectCache();
            logger.LogInformation("Project with Id: {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
