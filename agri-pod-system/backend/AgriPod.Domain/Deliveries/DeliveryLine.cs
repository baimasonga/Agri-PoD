using AgriPod.Domain.Common;

namespace AgriPod.Domain.Deliveries;

public sealed class DeliveryLine : Entity
{
    private DeliveryLine()
    {
    }

    public DeliveryLine(Guid deliveryId, Guid inventoryItemId, decimal quantity, string? barcode)
    {
        DeliveryId = deliveryId;
        InventoryItemId = inventoryItemId;
        Quantity = quantity;
        Barcode = barcode?.Trim();
    }

    public Guid DeliveryId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public decimal Quantity { get; private set; }
    public string? Barcode { get; private set; }
}
