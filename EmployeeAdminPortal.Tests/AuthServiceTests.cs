using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories.Interfaces;
using EmployeeAdminPortal.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EmployeeAdminPortal.Tests;

public class AuthServiceTests
{
    private static IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Token"] = "ThisIsAVeryLongTestSecretKeyForJwtAuthenticationThatIsLongEnoughForHmacSha5121234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        var employee = new Employee { Id = Guid.NewGuid(), Name = "Test Employee", Email = "employee@test.com", PasswordHash = string.Empty };
        employee.PasswordHash = new PasswordHasher<Employee>().HashPassword(employee, "Password123!");

        var authRepository = new Mock<IAuthRepository>();
        authRepository.Setup(r => r.FindByEmailAsync(employee.Email)).ReturnsAsync(employee);

        var service = new AuthService(authRepository.Object, GetConfiguration());

        var token = await service.LoginAsync(employee.Email, "Password123!");

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenEmailIsInvalid()
    {
        var authRepository = new Mock<IAuthRepository>();
        authRepository.Setup(r => r.FindByEmailAsync("missing@test.com")).ReturnsAsync((Employee?)null);

        var service = new AuthService(authRepository.Object, GetConfiguration());

        var token = await service.LoginAsync("missing@test.com", "Password123!");

        Assert.Null(token);
    }
}