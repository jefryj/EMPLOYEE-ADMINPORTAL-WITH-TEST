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
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService departmentService;
        private readonly ILogger<DepartmentsController> logger;

        public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
        {
            this.departmentService = departmentService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDepartments([FromQuery] DepartmentSearchDto searchDto)
        {
            logger.LogInformation("GetAllDepartments called with Search={Search}, PageNumber={PageNumber}, PageSize={PageSize}", searchDto.Search, searchDto.PageNumber, searchDto.PageSize);
            var departments = await departmentService.GetAllDepartmentsAsync(searchDto);
            logger.LogInformation("Retrieved {Count} departments", departments.Count);
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            logger.LogInformation("GetDepartmentById endpoint called with Id: {Id}", id);
            var department = await departmentService.GetDepartmentByIdAsync(id);
            if (department is null)
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
            if (dto is null)
            {
                logger.LogWarning("AddDepartment endpoint called with null DepartmentDto");
                throw new ArgumentException("Department data is required.");
            }

            var department = await departmentService.AddDepartmentAsync(dto, User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown");
            logger.LogInformation("Department added successfully with Id: {Id}", department.Id);
            return CreatedAtAction(nameof(GetDepartmentById), new { id = department.Id }, department);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, DepartmentDto dto)
        {
            logger.LogInformation("UpdateDepartment endpoint called with Id: {Id}, DepartmentName: {DepartmentName}", id, dto?.DepartmentName);
            if (dto is null)
            {
                logger.LogWarning("UpdateDepartment endpoint called with null DepartmentDto for Id: {Id}", id);
                throw new ArgumentException("Department data is required.");
            }

            var department = await departmentService.UpdateDepartmentAsync(id, dto, User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown");
            if (department is null)
            {
                logger.LogWarning("Department with Id: {Id} not found", id);
                throw new KeyNotFoundException("Department not found");
            }

            logger.LogInformation("Department with Id: {Id} updated successfully", id);
            return Ok(department);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            logger.LogInformation("DeleteDepartment endpoint called with Id: {Id}", id);
            var deleted = await departmentService.DeleteDepartmentAsync(id, User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown");
            if (!deleted)
            {
                logger.LogWarning("Department with Id: {Id} not found", id);
                throw new KeyNotFoundException("Department not found");
            }

            logger.LogInformation("Department with Id: {Id} deleted successfully", id);
            return NoContent();
        }
    }
}