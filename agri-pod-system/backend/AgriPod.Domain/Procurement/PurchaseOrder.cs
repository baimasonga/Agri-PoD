using AgriPod.Domain.Common;

namespace AgriPod.Domain.Procurement;

public sealed class PurchaseOrder : Entity
{
    private PurchaseOrder()
    {
    }

    public PurchaseOrder(string purchaseNumber, Guid supplierId, Guid inventoryItemId, decimal quantity, string batchNumber)
    {
        PurchaseNumber = purchaseNumber.Trim().ToUpperInvariant();
        SupplierId = supplierId;
        InventoryItemId = inventoryItemId;
        Quantity = quantity;
        BatchNumber = batchNumber.Trim().ToUpperInvariant();
    }

    public string PurchaseNumber { get; private set; } = "";
    public Guid SupplierId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public decimal Quantity { get; private set; }
    public string BatchNumber { get; private set; } = "";
    public bool SubmittedForReceipt { get; private set; }

    public void SubmitForReceipt()
    {
        SubmittedForReceipt = true;
        Touch();
    }
}
