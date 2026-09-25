namespace EmployeeAdminPortal.Repositories.Interfaces;

public sealed record DepartmentSummaryItem(string DepartmentName, int EmployeeCount, decimal AverageSalary, decimal MinSalary, decimal MaxSalary);

public sealed record ProjectSummaryItem(string ProjectName, int ProjectMembersCount, decimal AverageSalary, decimal MinSalary, decimal MaxSalary);

public interface IReportRepository
{
    Task<List<DepartmentSummaryItem>> GetDepartmentSummaryAsync();
    Task<List<ProjectSummaryItem>> GetProjectSummaryAsync();
}