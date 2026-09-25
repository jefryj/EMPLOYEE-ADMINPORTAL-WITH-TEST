using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories.Interfaces;
using EmployeeAdminPortal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EmployeeAdminPortal.Services;

public class ProjectService(
    IProjectRepository projectRepository,
    IAuditLogRepository auditLogRepository,
    IMemoryCache cache,
    ILogger<ProjectService> logger) : IProjectService
{
    private static readonly List<string> CacheKeys = new();

    public async Task<List<Project>> GetAllProjectsAsync(ProjectSearchDto searchDto)
    {
        IQueryable<Project> query = projectRepository.Query();

        if (!string.IsNullOrWhiteSpace(searchDto.Search))
        {
            query = query.Where(p => p.ProjectName.Contains(searchDto.Search));
        }

        var cacheKey = $"projects-{searchDto.Search}-{searchDto.PageNumber}-{searchDto.PageSize}";
        if (cache.TryGetValue(cacheKey, out List<Project>? projects) && projects is not null)
        {
            logger.LogInformation("Project list cache hit for key {CacheKey}", cacheKey);
            return projects;
        }

        logger.LogInformation("Project list cache miss for key {CacheKey}", cacheKey);
        projects = await query.OrderBy(p => p.Id)
            .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        cache.Set(cacheKey, projects, TimeSpan.FromMinutes(5));
        if (!CacheKeys.Contains(cacheKey))
        {
            CacheKeys.Add(cacheKey);
        }

        return projects;
    }

    public Task<Project?> GetProjectByIdAsync(int id) => projectRepository.FindAsync(id);

    public async Task<Project> AddProjectAsync(ProjectDto dto, string userName)
    {
        if (await projectRepository.AnyByNameAsync(dto.ProjectName))
        {
            throw new ArgumentException("Project already exists.");
        }

        var project = new Project { ProjectName = dto.ProjectName, ProjectMembersCount = 0 };
        await projectRepository.AddAsync(project);
        await projectRepository.SaveChangesAsync();

        await AddAuditLogAsync(userName, "Create", "Project", $"Project {project.ProjectName} was created");
        ClearCache();
        return project;
    }

    public async Task<Project?> UpdateProjectAsync(int id, ProjectDto dto, string userName)
    {
        var project = await projectRepository.FindAsync(id);
        if (project is null)
        {
            return null;
        }

        if (await projectRepository.AnyByNameExcludingIdAsync(id, dto.ProjectName))
        {
            throw new ArgumentException("Project already exists.");
        }

        project.ProjectName = dto.ProjectName;
        projectRepository.Update(project);
        await projectRepository.SaveChangesAsync();

        await AddAuditLogAsync(userName, "Update", "Project", $"Project {project.ProjectName} was updated");
        ClearCache();
        return project;
    }

    public async Task<bool> DeleteProjectAsync(int id, string userName)
    {
        var project = await projectRepository.FindAsync(id);
        if (project is null)
        {
            return false;
        }

        projectRepository.Remove(project);
        await projectRepository.SaveChangesAsync();

        await AddAuditLogAsync(userName, "Delete", "Project", $"Project {project.ProjectName} was deleted");
        ClearCache();
        return true;
    }

    private async Task AddAuditLogAsync(string userName, string action, string entityName, string details)
    {
        await auditLogRepository.AddAsync(new AuditLog
        {
            UserName = userName,
            Action = action,
            EntityName = entityName,
            Details = details,
            CreatedAt = DateTime.UtcNow
        });

        await auditLogRepository.SaveChangesAsync();
    }

    private void ClearCache()
    {
        logger.LogInformation("Clearing project cache with {Count} entries", CacheKeys.Count);
        foreach (var key in CacheKeys)
        {
            cache.Remove(key);
        }

        CacheKeys.Clear();
    }
}