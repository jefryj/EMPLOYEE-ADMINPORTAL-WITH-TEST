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
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService employeeService;
        private readonly ILogger<EmployeesController> logger;

        public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
        {
            this.employeeService = employeeService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees([FromQuery] EmployeeSearchDto searchDto)
        {
            logger.LogInformation("GetAllEmployees called with Search={Search}, DepartmentId={DepartmentId}", searchDto.Search, searchDto.DepartmentId);
            var employees = await employeeService.GetAllEmployeesAsync(searchDto);
            logger.LogInformation("Retrieved {Count} employees", employees.Count);
            return Ok(employees);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddEmployee(AddEmployeeDto addemp)
        {
            if (addemp is null)
            {
                logger.LogWarning("AddEmployee endpoint called with null AddEmployeeDto");
                return BadRequest("Employee data is required.");
            }

            logger.LogInformation("Adding a new employee with Name: {Name}, Email: {Email}", addemp.Name, addemp.Email);
            var result = await employeeService.AddEmployeeAsync(addemp, User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown");
            if (!result.Success)
            {
                logger.LogWarning("Employee creation failed. Reason: {Reason}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            logger.LogInformation("Employee added successfully with Id: {Id}", result.Employee!.Id);
            return CreatedAtAction(nameof(GetAllEmployeesById), new { id = result.Employee.Id }, result.Employee);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllEmployeesById(Guid id)
        {
            logger.LogInformation("GetAllEmployeesById endpoint called with Id: {Id}", id);
            var employee = await employeeService.GetEmployeeByIdAsync(id);
            if (employee is null)
            {
                logger.LogWarning("Employee with Id: {Id} not found", id);
                throw new KeyNotFoundException("Employee not found");
            }

            logger.LogInformation("Employee with Id: {Id} retrieved successfully", id);
            return Ok(employee);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, UpdateEmployeeDto updateEmp)
        {
            if (updateEmp is null)
            {
                logger.LogWarning("UpdateEmployee endpoint called with null UpdateEmployeeDto");
                return BadRequest("Employee data is required.");
            }

            logger.LogInformation("updateEmployee endpoint called with Id: {Id}", id);
            var result = await employeeService.UpdateEmployeeAsync(id, updateEmp, User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown");
            if (!result.Success)
            {
                if (string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    logger.LogWarning("Employee with Id: {Id} not found", id);
                    throw new KeyNotFoundException("Employee not found");
                }

                logger.LogWarning("Employee update failed. Reason: {Reason}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            logger.LogInformation("Employee with Id: {Id} updated successfully", id);
            return Ok(result.Employee);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            logger.LogInformation("DeleteEmployee endpoint called with Id: {Id}", id);
            var deleted = await employeeService.DeleteEmployeeAsync(id, User?.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown");
            if (!deleted)
            {
                logger.LogWarning("Employee with Id: {Id} not found", id);
                throw new KeyNotFoundException("Employee not found");
            }

            logger.LogInformation("Employee with Id: {Id} deleted successfully", id);
            return NoContent();
        }

        [Authorize(Roles = "Employee")]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var result = await employeeService.ChangePasswordAsync(User?.FindFirst(ClaimTypes.Email)?.Value, request);
            if (!result.Success)
            {
                if (result.Message == "Employee not found")
                {
                    return NotFound(result.Message);
                }

                return BadRequest(result.Message);
            }

            logger.LogInformation("Password changed successfully for email {Email}", User?.FindFirst(ClaimTypes.Email)?.Value);
            return Ok(result.Message);
        }
    }
}