using EmployeeAdminPortal.Controllers;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeeAdminPortal.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_ReturnsToken_WhenCredentialsAreValid()
    {
        var authService = new Mock<IAuthService>();
        authService.Setup(s => s.LoginAsync("employee@test.com", "Password123!")).ReturnsAsync("token-value");

        var controller = new AuthController(authService.Object, Mock.Of<ILogger<AuthController>>());

        var result = await controller.Login(new LoginDto { Email = "employee@test.com", Password = "Password123!" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("token-value", okResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenEmailIsInvalid()
    {
        var authService = new Mock<IAuthService>();
        authService.Setup(s => s.LoginAsync("missing@test.com", "Password123!")).ReturnsAsync((string?)null);

        var controller = new AuthController(authService.Object, Mock.Of<ILogger<AuthController>>());

        var result = await controller.Login(new LoginDto { Email = "missing@test.com", Password = "Password123!" });

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid email or password", unauthorizedResult.Value);
    }
}