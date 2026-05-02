using AgriPod.Application.Sync;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdministrator,FieldOfficer,DistrictCoordinator")]
[Route("api/v1/sync")]
public sealed class SyncController(SyncService syncService) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<SyncResponse>(StatusCodes.Status200OK)]
    public async Task<SyncResponse> Sync(SyncRequest request, CancellationToken cancellationToken) =>
        await syncService.ApplyAsync(request, cancellationToken);
}
