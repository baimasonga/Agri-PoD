using AgriPod.Domain.Common;

namespace AgriPod.Domain.Campaigns;

public sealed class FarmerAllocation : Entity
{
    private FarmerAllocation()
    {
    }

    public FarmerAllocation(Guid campaignId, Guid farmerId, Guid inventoryItemId, decimal allocatedQuantity)
    {
        CampaignId = campaignId;
        FarmerId = farmerId;
        InventoryItemId = inventoryItemId;
        AllocatedQuantity = allocatedQuantity;
        AllocationToken = $"ALLOC-{Guid.NewGuid():N}";
    }

    public Guid CampaignId { get; private set; }
    public Guid FarmerId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public decimal AllocatedQuantity { get; private set; }
    public decimal DeliveredQuantity { get; private set; }
    public string AllocationToken { get; private set; } = "";

    public bool CanDeliver(decimal quantity) => DeliveredQuantity + quantity <= AllocatedQuantity;

    public void MarkDelivered(decimal quantity)
    {
        if (!CanDeliver(quantity))
        {
            throw new InvalidOperationException("Farmer cannot receive more than allocated inputs.");
        }

        DeliveredQuantity += quantity;
        Touch();
    }
}
