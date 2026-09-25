using EmployeeAdminPortal.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService reportService;
        private readonly ILogger<ReportsController> logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            this.reportService = reportService;
            this.logger = logger;
        }

        [HttpGet("department-summary")]
        public async Task<IActionResult> GetDepartmentSummary()
        {
            logger.LogInformation("GetDepartmentSummary endpoint called");
            return Ok(await reportService.GetDepartmentSummaryAsync());
        }

        [HttpGet("project-summary")]
        public async Task<IActionResult> GetProjectSummary()
        {
            logger.LogInformation("GetProjectSummary endpoint called");
            return Ok(await reportService.GetProjectSummaryAsync());
        }
    }
}