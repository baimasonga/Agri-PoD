using AgriPod.Application.Administration;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdministrator,Auditor,DistrictCoordinator")]
[Route("api/v1/administration")]
public sealed class AdministrationController(AdministrationService administrationService) : ControllerBase
{
    [HttpGet("districts")]
    public async Task<IReadOnlyCollection<DistrictDto>> Districts(CancellationToken cancellationToken) =>
        await administrationService.DistrictsAsync(cancellationToken);

    [HttpPost("districts")]
    public async Task<ActionResult<DistrictDto>> CreateDistrict(CreateDistrictRequest request, CancellationToken cancellationToken) =>
        Ok(await administrationService.CreateDistrictAsync(request, cancellationToken));

    [HttpGet("chiefdoms")]
    public async Task<IReadOnlyCollection<ChiefdomDto>> Chiefdoms(CancellationToken cancellationToken) =>
        await administrationService.ChiefdomsAsync(cancellationToken);

    [HttpPost("chiefdoms")]
    public async Task<ActionResult<ChiefdomDto>> CreateChiefdom(CreateChiefdomRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await administrationService.CreateChiefdomAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("settings")]
    public async Task<SystemSettingsDto> Settings(CancellationToken cancellationToken) =>
        await administrationService.SettingsAsync(cancellationToken);

    [HttpPut("settings")]
    public async Task<ActionResult<SystemSettingsDto>> UpdateSettings(UpdateSystemSettingsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await administrationService.UpdateSettingsAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("devices")]
    public async Task<IReadOnlyCollection<DeviceBindingDto>> DeviceBindings(CancellationToken cancellationToken) =>
        await administrationService.DeviceBindingsAsync(cancellationToken);

    [HttpPost("devices/bind")]
    public async Task<ActionResult<DeviceBindingDto>> BindDevice(RegisterDeviceBindingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await administrationService.BindDeviceAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
