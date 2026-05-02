using AgriPod.Application.Deliveries;
using AgriPod.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Route("api/v1/proof-of-delivery")]
public sealed class ProofOfDeliveryController(ProofOfDeliveryService proofOfDeliveryService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProofOfDeliveryResult>> Confirm(ProofOfDeliveryRequest request, CancellationToken cancellationToken)
    {
        var result = await proofOfDeliveryService.ConfirmAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result) : UnprocessableEntity(result);
    }
}
