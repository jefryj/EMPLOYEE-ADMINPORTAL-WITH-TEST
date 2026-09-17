using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration configuration;
        private readonly ILogger<AuthController> logger;

        public AuthController(ApplicationDbContext dbContext, IConfiguration configuration, ILogger<AuthController> logger)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
            this.logger = logger;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            logger.LogInformation("Login attempt for email {Email}", request.Email);
            var employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.Email == request.Email);
            if (employee == null)
            {
                logger.LogWarning("Login failed because employee email {Email} was not found", request.Email);
                return Unauthorized("Invalid email or password");
            }
            var passwordVerificationResult = new PasswordHasher<Employee>().VerifyHashedPassword(employee, employee.PasswordHash, request.Password);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                logger.LogWarning("Login failed because of an invalid password for email {Email}", request.Email);
                return Unauthorized("Invalid email or password");
            }

            var token = CreateToken(employee);
            logger.LogInformation("Login succeeded for employee {EmployeeId}", employee.Id);
            return Ok(token);
        }
        private string CreateToken(Employee employee)
        {
            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier,employee.Id.ToString()),

            new Claim(ClaimTypes.Email,employee.Email),

            new Claim(ClaimTypes.Role,employee.Role)
            };

            var key= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Token"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}