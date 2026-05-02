using AgriPod.Application.Abstractions;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdministrator,FieldOfficer,DistrictCoordinator")]
[Route("api/v1/otp")]
public sealed class OtpController(IOtpProvider otpProvider) : ControllerBase
{
    [HttpPost("send")]
    [ProducesResponseType<OtpResult>(StatusCodes.Status200OK)]
    public async Task<OtpResult> Send(SendOtpRequest request, CancellationToken cancellationToken) =>
        await otpProvider.SendAsync(request.RecipientPhone, request.Channel, cancellationToken);

    [HttpPost("verify")]
    [ProducesResponseType<OtpResult>(StatusCodes.Status200OK)]
    public async Task<OtpResult> Verify(VerifyOtpRequest request, CancellationToken cancellationToken) =>
        await otpProvider.VerifyAsync(request.RecipientPhone, request.Code, cancellationToken);
}
