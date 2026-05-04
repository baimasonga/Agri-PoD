using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AgriPod.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/auth")]
public sealed class AuthController(IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<AuthTokenResponse>(StatusCodes.Status200OK)]
    public ActionResult<AuthTokenResponse> Login(LoginRequest request)
    {
        var role = ResolveRole(request.Email);

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

    private static string ResolveRole(string email)
    {
        if (email.Contains("audit", StringComparison.OrdinalIgnoreCase)) return "Auditor";
        if (email.Contains("field", StringComparison.OrdinalIgnoreCase)) return "FieldOfficer";
        if (email.Contains("coordinator", StringComparison.OrdinalIgnoreCase)) return "DistrictCoordinator";
        if (email.Contains("warehouse", StringComparison.OrdinalIgnoreCase)) return "WarehouseManager";
        if (email.Contains("procurement", StringComparison.OrdinalIgnoreCase)) return "ProcurementOfficer";
        if (email.Contains("manager", StringComparison.OrdinalIgnoreCase)) return "ProjectManager";
        if (email.Contains("driver", StringComparison.OrdinalIgnoreCase)) return "Driver";
        if (email.Contains("me", StringComparison.OrdinalIgnoreCase)) return "MonitoringOfficer";
        if (email.Contains("viewer", StringComparison.OrdinalIgnoreCase)) return "Viewer";
        return "SystemAdmin";
    }
}
