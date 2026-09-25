using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories.Interfaces;
using EmployeeAdminPortal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EmployeeAdminPortal.Services;

public class DepartmentService(
    IDepartmentRepository departmentRepository,
    IAuditLogRepository auditLogRepository,
    IMemoryCache cache,
    ILogger<DepartmentService> logger) : IDepartmentService
{
    private static readonly List<string> CacheKeys = new();

    public async Task<List<Department>> GetAllDepartmentsAsync(DepartmentSearchDto searchDto)
    {
        IQueryable<Department> query = departmentRepository.Query();

        if (!string.IsNullOrWhiteSpace(searchDto.Search))
        {
            query = query.Where(d => d.DepartmentName.Contains(searchDto.Search));
        }

        var cacheKey = $"departments-{searchDto.Search}-{searchDto.PageNumber}-{searchDto.PageSize}";
        if (cache.TryGetValue(cacheKey, out List<Department>? departments) && departments is not null)
        {
            logger.LogInformation("Department list cache hit for key {CacheKey}", cacheKey);
            return departments;
        }

        logger.LogInformation("Department list cache miss for key {CacheKey}", cacheKey);
        departments = await query.OrderBy(d => d.Id)
            .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .ToListAsync();

        cache.Set(cacheKey, departments, TimeSpan.FromMinutes(5));
        if (!CacheKeys.Contains(cacheKey))
        {
            CacheKeys.Add(cacheKey);
        }

        return departments;
    }

    public Task<Department?> GetDepartmentByIdAsync(int id) => departmentRepository.FindAsync(id);

    public async Task<Department> AddDepartmentAsync(DepartmentDto dto, string userName)
    {
        if (await departmentRepository.AnyByNameAsync(dto.DepartmentName))
        {
            throw new ArgumentException("Department already exists.");
        }

        var department = new Department { DepartmentName = dto.DepartmentName };
        await departmentRepository.AddAsync(department);
        await departmentRepository.SaveChangesAsync();

        await AddAuditLogAsync(userName, "Create", "Department", $"Department {department.DepartmentName} was created");
        ClearCache();
        return department;
    }

    public async Task<Department?> UpdateDepartmentAsync(int id, DepartmentDto dto, string userName)
    {
        var department = await departmentRepository.FindAsync(id);
        if (department is null)
        {
            return null;
        }

        if (await departmentRepository.AnyByNameExcludingIdAsync(id, dto.DepartmentName))
        {
            throw new ArgumentException("Department already exists.");
        }

        department.DepartmentName = dto.DepartmentName;
        departmentRepository.Update(department);
        await departmentRepository.SaveChangesAsync();

        await AddAuditLogAsync(userName, "Update", "Department", $"Department {department.DepartmentName} was updated");
        ClearCache();
        return department;
    }

    public async Task<bool> DeleteDepartmentAsync(int id, string userName)
    {
        var department = await departmentRepository.FindAsync(id);
        if (department is null)
        {
            return false;
        }

        departmentRepository.Remove(department);
        await departmentRepository.SaveChangesAsync();

        await AddAuditLogAsync(userName, "Delete", "Department", $"Department {department.DepartmentName} was deleted");
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
        logger.LogInformation("Clearing department cache with {Count} entries", CacheKeys.Count);
        foreach (var key in CacheKeys)
        {
            cache.Remove(key);
        }

        CacheKeys.Clear();
    }
}