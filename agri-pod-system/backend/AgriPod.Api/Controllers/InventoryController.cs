using AgriPod.Application.Inventory;
using AgriPod.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AgriPod.Api.Controllers;

[ApiController]
[Route("api/v1/inventory-items")]
public sealed class InventoryController(InventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<InventoryItemDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyCollection<InventoryItemDto>> List(CancellationToken cancellationToken) =>
        await inventoryService.ListItemsAsync(cancellationToken);

    [HttpPost]
    [ProducesResponseType<InventoryItemDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<InventoryItemDto>> Create(CreateInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var item = await inventoryService.CreateItemAsync(request, cancellationToken);
        return CreatedAtAction(nameof(List), new { id = item.Id }, item);
    }

    [HttpGet("/api/v1/warehouses")]
    [ProducesResponseType<IReadOnlyCollection<WarehouseDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyCollection<WarehouseDto>> Warehouses(CancellationToken cancellationToken) =>
        await inventoryService.ListWarehousesAsync(cancellationToken);

    [HttpPost("/api/v1/warehouses")]
    [ProducesResponseType<WarehouseDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<WarehouseDto>> CreateWarehouse(CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var warehouse = await inventoryService.CreateWarehouseAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Warehouses), new { id = warehouse.Id }, warehouse);
    }

    [HttpGet("/api/v1/stock-lots")]
    [ProducesResponseType<IReadOnlyCollection<StockLotDto>>(StatusCodes.Status200OK)]
    public async Task<IReadOnlyCollection<StockLotDto>> StockLots(CancellationToken cancellationToken) =>
        await inventoryService.ListStockLotsAsync(cancellationToken);
}
