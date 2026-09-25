using EmployeeAdminPortal.Repositories.Interfaces;

namespace EmployeeAdminPortal.Services.Interfaces;

public interface IReportService
{
    Task<List<DepartmentSummaryItem>> GetDepartmentSummaryAsync();
    Task<List<ProjectSummaryItem>> GetProjectSummaryAsync();
}