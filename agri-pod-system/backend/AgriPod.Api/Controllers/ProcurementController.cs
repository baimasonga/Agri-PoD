using AgriPod.Application.Procurement;
using AgriPod.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Authorize(Roles = "SystemAdmin,ProcurementOfficer")]
[Route("api/v1/procurement")]
public sealed class ProcurementController(ProcurementService procurementService) : ControllerBase
{
    [HttpGet("suppliers")]
    public async Task<IReadOnlyCollection<SupplierDto>> Suppliers(CancellationToken cancellationToken) =>
        await procurementService.SuppliersAsync(cancellationToken);

    [HttpPost("suppliers")]
    public async Task<ActionResult<SupplierDto>> CreateSupplier(CreateSupplierRequest request, CancellationToken cancellationToken) =>
        Ok(await procurementService.CreateSupplierAsync(request, cancellationToken));

    [HttpPost("purchase-orders")]
    public async Task<ActionResult<PurchaseOrderDto>> CreatePurchaseOrder(CreatePurchaseOrderRequest request, CancellationToken cancellationToken) =>
        Ok(await procurementService.CreatePurchaseOrderAsync(request, cancellationToken));

    [HttpGet("purchase-orders")]
    public async Task<IReadOnlyCollection<PurchaseOrderDto>> PurchaseOrders(CancellationToken cancellationToken) =>
        await procurementService.PurchaseOrdersAsync(cancellationToken);

    [HttpPost("stock-receipts")]
    public async Task<ActionResult<StockLotDto>> ReceiveStock(ReceiveStockRequest request, CancellationToken cancellationToken) =>
        Ok(await procurementService.ReceiveStockAsync(request, cancellationToken));
}
