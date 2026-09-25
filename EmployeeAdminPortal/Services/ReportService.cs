using EmployeeAdminPortal.Repositories.Interfaces;
using EmployeeAdminPortal.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EmployeeAdminPortal.Services;

public class ReportService(IReportRepository reportRepository, IMemoryCache cache, ILogger<ReportService> logger) : IReportService
{
    public async Task<List<DepartmentSummaryItem>> GetDepartmentSummaryAsync()
    {
        const string cacheKey = "department-summary";
        if (cache.TryGetValue(cacheKey, out List<DepartmentSummaryItem>? report) && report is not null)
        {
            logger.LogInformation("Department Summary CACHE HIT");
            return report;
        }

        logger.LogInformation("Department Summary CACHE MISS");
        report = await reportRepository.GetDepartmentSummaryAsync();
        cache.Set(cacheKey, report, TimeSpan.FromMinutes(5));
        return report;
    }

    public async Task<List<ProjectSummaryItem>> GetProjectSummaryAsync()
    {
        const string cacheKey = "project-summary";
        if (cache.TryGetValue(cacheKey, out List<ProjectSummaryItem>? report) && report is not null)
        {
            logger.LogInformation("Project Summary CACHE HIT");
            return report;
        }

        logger.LogInformation("Project Summary CACHE MISS");
        report = await reportRepository.GetProjectSummaryAsync();
        cache.Set(cacheKey, report, TimeSpan.FromMinutes(5));
        return report;
    }
}