using AgriPod.Application.Deliveries;
using AgriPod.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Route("api/v1/deliveries")]
public sealed class DeliveriesController(DeliveryService deliveryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<DeliveryDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyCollection<DeliveryDto>> List(CancellationToken cancellationToken) =>
        await deliveryService.ListAsync(cancellationToken);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<DeliveryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliveryDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var delivery = await deliveryService.GetAsync(id, cancellationToken);
        return delivery is null ? NotFound() : Ok(delivery);
    }

    [HttpPost]
    [ProducesResponseType<DeliveryDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<DeliveryDto>> Create(CreateDeliveryRequest request, CancellationToken cancellationToken)
    {
        var delivery = await deliveryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = delivery.Id }, delivery);
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType<DeliveryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliveryDto>> Complete(Guid id, CompleteDeliveryRequest request, CancellationToken cancellationToken)
    {
        var delivery = await deliveryService.CompleteAsync(id, request, cancellationToken);
        return delivery is null ? NotFound() : Ok(delivery);
    }
}
