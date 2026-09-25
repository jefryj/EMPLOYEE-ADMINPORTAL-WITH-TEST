using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly ILogger<AuthController> logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            this.authService = authService;
            this.logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            logger.LogInformation("Login attempt for email {Email}", request.Email);

            var token = await authService.LoginAsync(request.Email, request.Password);
            if (token is null)
            {
                logger.LogWarning("Login failed for email {Email}", request.Email);
                return Unauthorized("Invalid email or password");
            }

            logger.LogInformation("Login succeeded for email {Email}", request.Email);
            return Ok(token);
        }
    }
}