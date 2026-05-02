using AgriPod.Application.Abstractions;
using AgriPod.Domain.Inventory;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Inventory;

public sealed class InventoryService(IAgriPodDbContext db)
{
    public async Task<IReadOnlyCollection<InventoryItemDto>> ListItemsAsync(CancellationToken cancellationToken)
    {
        return await db.InventoryItems
            .OrderBy(item => item.Name)
            .Select(item => new InventoryItemDto(item.Id, item.Sku, item.Name, item.Category, item.UnitOfMeasure))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<WarehouseDto>> ListWarehousesAsync(CancellationToken cancellationToken) =>
        await db.Warehouses
            .OrderBy(warehouse => warehouse.District)
            .ThenBy(warehouse => warehouse.Name)
            .Select(warehouse => new WarehouseDto(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.District))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<StockLotDto>> ListStockLotsAsync(CancellationToken cancellationToken)
    {
        var warehouses = await db.Warehouses.ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);
        var lots = await db.StockLots.ToListAsync(cancellationToken);
        return lots
            .OrderBy(x => warehouses.GetValueOrDefault(x.WarehouseId, "UNKNOWN"))
            .ThenBy(x => x.LotCode)
            .Select(x => new StockLotDto(x.Id, x.ItemId, x.LotCode, warehouses.GetValueOrDefault(x.WarehouseId, "UNKNOWN"), x.Quantity, x.ExpiresOn))
            .ToList();
    }

    public async Task<InventoryItemDto> CreateItemAsync(CreateInventoryItemRequest request, CancellationToken cancellationToken)
    {
        var item = new InventoryItem(request.Sku, request.Name, request.Category, request.UnitOfMeasure);
        db.InventoryItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return new InventoryItemDto(item.Id, item.Sku, item.Name, item.Category, item.UnitOfMeasure);
    }

    public async Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var warehouse = new Warehouse(request.Code, request.Name, request.District);
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync(cancellationToken);
        return new WarehouseDto(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.District);
    }
}
