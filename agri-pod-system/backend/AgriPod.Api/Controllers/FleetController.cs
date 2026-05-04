using AgriPod.Application.Fleet;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdmin,ProjectManager,WarehouseManager,Driver,MonitoringOfficer,Auditor")]
[Route("api/v1/fleet")]
public sealed class FleetController(FleetTrackingService fleetTrackingService) : ControllerBase
{
    [HttpGet("locations/latest")]
    public async Task<IReadOnlyCollection<VehicleLocationRequest>> Latest(CancellationToken cancellationToken) =>
        await fleetTrackingService.LatestAsync(cancellationToken);

    [HttpPost("locations")]
    public async Task<IActionResult> Record(VehicleLocationRequest request, CancellationToken cancellationToken)
    {
        await fleetTrackingService.RecordLocationAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpPost("devices/register")]
    public async Task<ActionResult<VehicleDeviceDto>> RegisterDevice(RegisterVehicleDeviceRequest request, CancellationToken cancellationToken) =>
        Ok(await fleetTrackingService.RegisterDeviceAsync(request, cancellationToken));

    [HttpPost("pings/bulk")]
    public async Task<IActionResult> BulkRecord(BulkGpsPingRequest request, CancellationToken cancellationToken)
    {
        await fleetTrackingService.BulkRecordLocationAsync(request, cancellationToken);
        return Accepted();
    }
}
