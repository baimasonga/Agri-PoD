using AgriPod.Application.Dispatch;
using AgriPod.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Route("api/v1/dispatch")]
public sealed class DispatchController(DispatchService dispatchService) : ControllerBase
{
    [HttpGet("vehicles")]
    public async Task<IReadOnlyCollection<VehicleDto>> Vehicles(CancellationToken cancellationToken) =>
        await dispatchService.VehiclesAsync(cancellationToken);

    [HttpPost("vehicles")]
    public async Task<ActionResult<VehicleDto>> RegisterVehicle(RegisterVehicleRequest request, CancellationToken cancellationToken) =>
        Ok(await dispatchService.RegisterVehicleAsync(request, cancellationToken));

    [HttpGet]
    public async Task<IReadOnlyCollection<DispatchDto>> Dispatches(CancellationToken cancellationToken) =>
        await dispatchService.DispatchesAsync(cancellationToken);

    [HttpPost]
    public async Task<ActionResult<DispatchDto>> CreateDispatch(CreateDispatchRequest request, CancellationToken cancellationToken) =>
        Ok(await dispatchService.CreateDispatchAsync(request, cancellationToken));

    [HttpPost("{id:guid}/confirm-loaded")]
    public async Task<ActionResult<DispatchDto>> ConfirmLoaded(Guid id, CancellationToken cancellationToken)
    {
        var dispatch = await dispatchService.ConfirmLoadedAsync(id, cancellationToken);
        return dispatch is null ? NotFound() : Ok(dispatch);
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<DistributionSessionDto>> StartSession(StartDistributionSessionRequest request, CancellationToken cancellationToken)
    {
        var session = await dispatchService.StartSessionAsync(request, cancellationToken);
        return session is null ? BadRequest("Manifest barcode or dispatch could not be validated.") : Ok(session);
    }
}
