using AgriPod.Domain.Common;

namespace AgriPod.Domain.Inventory;

public sealed class StockLot : Entity
{
    private StockLot()
    {
    }

    public StockLot(Guid itemId, Guid warehouseId, string lotCode, decimal quantity, DateOnly? expiresOn)
    {
        ItemId = itemId;
        WarehouseId = warehouseId;
        LotCode = lotCode.Trim().ToUpperInvariant();
        Quantity = quantity;
        ExpiresOn = expiresOn;
    }

    public Guid ItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string LotCode { get; private set; } = "";
    public decimal Quantity { get; private set; }
    public DateOnly? ExpiresOn { get; private set; }

    public void Adjust(decimal delta)
    {
        Quantity += delta;
        Touch();
    }
}
