using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories.Interfaces;
using EmployeeAdminPortal.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeAdminPortal.Services;

public class AuthService(IAuthRepository authRepository, IConfiguration configuration) : IAuthService
{
    public async Task<string?> LoginAsync(string email, string password)
    {
        var employee = await authRepository.FindByEmailAsync(email);
        if (employee is null)
        {
            return null;
        }

        var passwordVerificationResult = new PasswordHasher<Employee>().VerifyHashedPassword(employee, employee.PasswordHash, password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return CreateToken(employee);
    }

    private string CreateToken(Employee employee)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim(ClaimTypes.Email, employee.Email),
            new Claim(ClaimTypes.Role, employee.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Token"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}