using EmployeeAdminPortal.Repositories.Interfaces;
using EmployeeAdminPortal.Services.Interfaces;

namespace EmployeeAdminPortal.Services;

public class ReportService(IReportRepository reportRepository, ILogger<ReportService> logger) : IReportService
{
    public async Task<List<DepartmentSummaryItem>> GetDepartmentSummaryAsync()
    {
        logger.LogInformation("Fetching Department Summary from database");

        return await reportRepository.GetDepartmentSummaryAsync();
    }

    public async Task<List<ProjectSummaryItem>> GetProjectSummaryAsync()
    {
        logger.LogInformation("Fetching Project Summary from database");

        return await reportRepository.GetProjectSummaryAsync();
    }
}