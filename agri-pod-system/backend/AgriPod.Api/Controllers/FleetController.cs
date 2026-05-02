using AgriPod.Application.Fleet;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdministrator,Driver,ProjectManager,DistrictCoordinator,Auditor,MonitoringEvaluationOfficer")]
[Route("api/v1/fleet")]
public sealed class FleetController(FleetTrackingService fleetTrackingService) : ControllerBase
{
    [HttpGet("locations/latest")]
    [ProducesResponseType<IReadOnlyCollection<VehicleLocationRequest>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyCollection<VehicleLocationRequest>> Latest(CancellationToken cancellationToken) =>
        await fleetTrackingService.LatestAsync(cancellationToken);

    [HttpPost("locations")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Record(VehicleLocationRequest request, CancellationToken cancellationToken)
    {
        await fleetTrackingService.RecordLocationAsync(request, cancellationToken);
        return Accepted();
    }
}
