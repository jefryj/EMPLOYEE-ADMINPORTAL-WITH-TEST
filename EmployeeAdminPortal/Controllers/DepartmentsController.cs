using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;


namespace EmployeeAdminPortal.Controllers
{   
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {   
        private static readonly List<string> cacheKeys = new();
        private readonly IMemoryCache cache;
        private readonly ApplicationDbContext dBcontext;
        private readonly ILogger<DepartmentsController> logger;

        public DepartmentsController(ApplicationDbContext dBcontext, IMemoryCache cache, ILogger<DepartmentsController> logger)
        {
            this.dBcontext = dBcontext;
            this.cache = cache;
            this.logger = logger;
        }

        private void ClearDepartmentCache()
            {
                logger.LogInformation("Clearing department cache with {Count} entries", cacheKeys.Count);
                foreach (var key in cacheKeys)
                {
                    cache.Remove(key);
                }

                cacheKeys.Clear();
            }
        [HttpGet]
        public async Task<IActionResult> GetAllDepartments([FromQuery] DepartmentSearchDto searchDto)
        {
            logger.LogInformation("GetAllDepartments called with Search={Search}, PageNumber={PageNumber}, PageSize={PageSize}", searchDto.Search, searchDto.PageNumber, searchDto.PageSize);
            IQueryable<Department> query = dBcontext.Departments;

            if (!string.IsNullOrWhiteSpace(searchDto.Search))
            {
                logger.LogInformation("Applying department search filter: {Search}", searchDto.Search);
                query = query.Where(d => d.DepartmentName.Contains(searchDto.Search));
            }

            string cacheKey = $"departments-{searchDto.Search}-{searchDto.PageNumber}-{searchDto.PageSize}";

            List<Department>? departments;

            bool foundInCache = cache.TryGetValue(cacheKey, out departments);

            if (foundInCache == false)
            {
                logger.LogInformation("Department list cache miss for key {CacheKey}", cacheKey);
                departments = await query.OrderBy(d => d.Id).Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize).ToListAsync();

                cache.Set(cacheKey, departments, TimeSpan.FromMinutes(5));

                if (!cacheKeys.Contains(cacheKey))
                {
                    cacheKeys.Add(cacheKey);
                }
            }
            else
            {
                logger.LogInformation("Department list cache hit for key {CacheKey}", cacheKey);
            }

            logger.LogInformation("Retrieved {Count} departments", departments?.Count ?? 0);
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            logger.LogInformation("GetDepartmentById endpoint called with Id: {Id}", id);
            var department = await dBcontext.Departments.FindAsync(id);
            if(department == null)
            {
                logger.LogWarning("Department with Id: {Id} not found", id);
                throw new KeyNotFoundException("Department not found");
            }
            logger.LogInformation("Department with Id: {Id} retrieved successfully", id);
            return Ok(department);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]

        public async Task<IActionResult> AddDepartment(DepartmentDto dto)
        {
            logger.LogInformation("AddDepartment endpoint called with DepartmentName: {DepartmentName}", dto?.DepartmentName);
            if(dto == null)
            {
                logger.LogWarning("AddDepartment endpoint called with null DepartmentDto");
                throw new ArgumentException("Department data is required.");
            }
            bool departmentExists = await dBcontext.Departments.AnyAsync(d => d.DepartmentName == dto.DepartmentName);

            if (departmentExists)
            {
                logger.LogWarning("Department with name {DepartmentName} already exists", dto.DepartmentName);
                throw new ArgumentException("Department already exists.");
            }
            var department = new Department
            {
                DepartmentName = dto.DepartmentName
            };

            await dBcontext.Departments.AddAsync(department);
            await dBcontext.SaveChangesAsync();
            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Create",
                EntityName = "Department",
                Details = $"Department {department.DepartmentName} was created",
                CreatedAt = DateTime.UtcNow
            };

            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();
            ClearDepartmentCache();
            logger.LogInformation("Department added successfully with Id: {Id}", department.Id);
            return CreatedAtAction(nameof(GetDepartmentById), new { id = department.Id }, department);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateDepartment(int id, DepartmentDto dto)
        {   
            logger.LogInformation("UpdateDepartment endpoint called with Id: {Id}, DepartmentName: {DepartmentName}", id, dto?.DepartmentName);
            if(dto == null)
            {
                logger.LogWarning("UpdateDepartment endpoint called with null DepartmentDto for Id: {Id}", id);
                throw new ArgumentException("Department data is required.");
            }
            var department = await dBcontext.Departments.FindAsync(id);
            if (department == null)
            {
                logger.LogWarning("Department with Id: {Id} not found", id);
                throw new KeyNotFoundException("Department not found");
            }
            bool departmentExists = await dBcontext.Departments.AnyAsync(d =>d.Id != id && d.DepartmentName == dto.DepartmentName);

            if (departmentExists)
            {
                logger.LogWarning("Department with name {DepartmentName} already exists", dto.DepartmentName);
                throw new ArgumentException("Department already exists.");
            }
            department.DepartmentName = dto.DepartmentName;
            await dBcontext.SaveChangesAsync();
            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Update",
                EntityName = "Department",
                Details = $"Department {department.DepartmentName} was updated",
                CreatedAt = DateTime.UtcNow
            };
            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();
            ClearDepartmentCache();
            logger.LogInformation("Department with Id: {Id} updated successfully", id);
            return Ok(department);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            logger.LogInformation("DeleteDepartment endpoint called with Id: {Id}", id);
            var department = await dBcontext.Departments.FindAsync(id);
            if (department == null)
            {
                logger.LogWarning("Department with Id: {Id} not found", id);
                throw new KeyNotFoundException("Department not found");
            }
            dBcontext.Departments.Remove(department);
            await dBcontext.SaveChangesAsync();
            var auditLog = new AuditLog
            {
                UserName = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown",
                Action = "Delete",
                EntityName = "Department",
                Details = $"Department {department.DepartmentName} was deleted",
                CreatedAt = DateTime.UtcNow
            };
            await dBcontext.AuditLogs.AddAsync(auditLog);
            await dBcontext.SaveChangesAsync();
            ClearDepartmentCache();
            logger.LogInformation("Department with Id: {Id} deleted successfully", id);
            return NoContent();
        }
    }
}
