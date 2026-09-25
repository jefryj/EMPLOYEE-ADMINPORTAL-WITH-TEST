namespace EmployeeAdminPortal.Services.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(string email, string password);
}