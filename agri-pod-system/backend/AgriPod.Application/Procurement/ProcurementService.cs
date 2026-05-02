using AgriPod.Application.Abstractions;
using AgriPod.Domain.Inventory;
using AgriPod.Domain.Procurement;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Procurement;

public sealed class ProcurementService(IAgriPodDbContext db)
{
    public async Task<IReadOnlyCollection<SupplierDto>> SuppliersAsync(CancellationToken cancellationToken) =>
        await db.Suppliers.OrderBy(x => x.Name).Select(x => new SupplierDto(x.Id, x.Name, x.ContactPhone)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<PurchaseOrderDto>> PurchaseOrdersAsync(CancellationToken cancellationToken) =>
        await db.PurchaseOrders
            .OrderBy(x => x.PurchaseNumber)
            .Select(x => new PurchaseOrderDto(x.Id, x.PurchaseNumber, x.SupplierId, x.InventoryItemId, x.Quantity, x.BatchNumber, x.SubmittedForReceipt))
            .ToListAsync(cancellationToken);

    public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = new Supplier(request.Name, request.ContactPhone);
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(cancellationToken);
        return new SupplierDto(supplier.Id, supplier.Name, supplier.ContactPhone);
    }

    public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var order = new PurchaseOrder(request.PurchaseNumber, request.SupplierId, request.InventoryItemId, request.Quantity, request.BatchNumber);
        order.SubmitForReceipt();
        db.PurchaseOrders.Add(order);
        db.AuditLogs.Add(new("procurement-officer", "PurchaseOrderSubmitted", nameof(PurchaseOrder), order.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<StockLotDto> ReceiveStockAsync(ReceiveStockRequest request, CancellationToken cancellationToken)
    {
        var lot = new StockLot(request.InventoryItemId, request.WarehouseId, request.LotCode, request.Quantity, request.ExpiresOn);
        db.StockLots.Add(lot);
        db.AuditLogs.Add(new("warehouse-manager", "StockReceived", nameof(StockLot), lot.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return new StockLotDto(lot.Id, lot.ItemId, lot.LotCode, request.WarehouseId.ToString(), lot.Quantity, lot.ExpiresOn);
    }

    private static PurchaseOrderDto ToDto(PurchaseOrder order) =>
        new(order.Id, order.PurchaseNumber, order.SupplierId, order.InventoryItemId, order.Quantity, order.BatchNumber, order.SubmittedForReceipt);
}
