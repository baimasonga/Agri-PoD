using AgriPod.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AgriPod.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<AuthTokenResponse>(StatusCodes.Status200OK)]
    public ActionResult<AuthTokenResponse> Login(LoginRequest request)
    {
        var role = request.Email.Contains("audit", StringComparison.OrdinalIgnoreCase)
            ? "Auditor"
            : request.Email.Contains("field", StringComparison.OrdinalIgnoreCase)
                ? "FieldOfficer"
                : "SystemAdministrator";

        var expiresAt = DateTimeOffset.UtcNow.AddHours(8);
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:SigningKey"] ?? "development-only-signing-key-change-before-production");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Email),
            new Claim(ClaimTypes.Email, request.Email),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "AgriPod",
            audience: configuration["Jwt:Audience"] ?? "AgriPod.Clients",
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));

        return new AuthTokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, role);
    }
}
