using AgriPod.Application.Traceability;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdministrator,ProjectManager,WarehouseManager,DistrictCoordinator,Auditor,MonitoringEvaluationOfficer")]
[Route("api/v1/traceability")]
public sealed class TraceabilityController(TraceabilityService traceabilityService) : ControllerBase
{
    [HttpGet("barcodes")]
    public async Task<IReadOnlyCollection<BarcodeTokenDto>> BarcodeTokens(CancellationToken cancellationToken) =>
        await traceabilityService.BarcodeTokensAsync(cancellationToken);

    [HttpPost("barcodes")]
    public async Task<ActionResult<BarcodeTokenDto>> GenerateBarcode(CreateBarcodeTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await traceabilityService.GenerateBarcodeAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("reconciliations")]
    public async Task<IReadOnlyCollection<StockReconciliationDto>> StockReconciliations(CancellationToken cancellationToken) =>
        await traceabilityService.StockReconciliationsAsync(cancellationToken);

    [HttpGet("reconciliation/{campaignId:guid}")]
    public async Task<ActionResult<StockReconciliationDto>> PreviewReconciliation(Guid campaignId, [FromQuery] decimal returnedQuantity, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await traceabilityService.PreviewReconciliationAsync(campaignId, returnedQuantity, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("reconciliation")]
    public async Task<ActionResult<StockReconciliationDto>> SubmitReconciliation(CreateStockReconciliationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await traceabilityService.SubmitReconciliationAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
