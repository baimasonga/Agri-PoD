using AgriPod.Application.Farmers;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdmin,ProjectManager,DistrictCoordinator,FieldOfficer,MonitoringOfficer,Auditor,Viewer")]
[Route("api/v1/farmers")]
public sealed class FarmersController(FarmerService farmerService) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyCollection<FarmerDto>> List(CancellationToken cancellationToken) =>
        await farmerService.ListAsync(cancellationToken);

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<FarmerDto>> Register(RegisterFarmerRequest request, CancellationToken cancellationToken)
    {
        var farmer = await farmerService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(List), new { id = farmer.Id }, farmer);
    }

    [HttpPost("{id:guid}/review")]
    public async Task<ActionResult<FarmerDto>> Review(Guid id, ReviewFarmerRequest request, CancellationToken cancellationToken)
    {
        var farmer = await farmerService.ReviewAsync(id, request, cancellationToken);
        return farmer is null ? NotFound() : Ok(farmer);
    }
}
