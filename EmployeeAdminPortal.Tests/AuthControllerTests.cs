using EmployeeAdminPortal.Controllers;
using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeAdminPortal.Tests.Controllers;

public class AuthControllerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        return new ApplicationDbContext(options);
    }

    private AuthController GetController(ApplicationDbContext dbContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Token"] = "ThisIsAVeryLongTestSecretKeyForJwtAuthenticationThatIsLongEnoughForHmacSha5121234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();

        var logger = Mock.Of<ILogger<AuthController>>();

        return new AuthController(dbContext,configuration,logger);
    }

    [Fact]
    public async Task Login_ReturnsToken_WhenCredentialsAreValid()
    {
        var dbContext = GetDbContext();
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = "Test Employee",
            Email = "employee@test.com",
            PasswordHash = string.Empty
        };
        employee.PasswordHash = new PasswordHasher<Employee>().HashPassword(employee, "Password123!");

        await dbContext.Employees.AddAsync(employee);
        await dbContext.SaveChangesAsync();

        var controller = GetController(dbContext);

        var result = await controller.Login(new LoginDto
        {
            Email = employee.Email,
            Password = "Password123!"
        });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotEmpty(okResult.Value!.ToString()!);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenEmailIsInvalid()
    {
        var controller = GetController(GetDbContext());

        var result = await controller.Login(new LoginDto
        {
            Email = "missing@test.com",
            Password = "Password123!"
        });

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid email or password", unauthorizedResult.Value);
    }
}
