using EmployeeAdminPortal.Models.Entities;

namespace EmployeeAdminPortal.Repositories.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task SaveChangesAsync();
}