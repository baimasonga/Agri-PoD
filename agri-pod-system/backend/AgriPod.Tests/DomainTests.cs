using AgriPod.Domain.Deliveries;
using AgriPod.Domain.Campaigns;
using AgriPod.Domain.Inventory;

namespace AgriPod.Tests;

public sealed class DomainTests
{
    [Fact]
    public void Inventory_item_normalizes_sku()
    {
        var item = new InventoryItem(" seed-rice-50 ", "Rice seed", "Seed", "kg");

        Assert.Equal("SEED-RICE-50", item.Sku);
    }

    [Fact]
    public void Delivery_completion_records_proof_event()
    {
        var delivery = new Delivery("pod-001", Guid.NewGuid(), "Recipient", "+23276000000", "Bo");

        delivery.MarkDelivered(8.484m, -13.229m, "field-user", "face://capture/001", DateTimeOffset.UtcNow, null, "photo://evidence/001", Guid.NewGuid().ToString());

        Assert.Equal(DeliveryStatus.Delivered, delivery.Status);
        Assert.Single(delivery.ProofEvents);
    }

    [Fact]
    public void Allocation_rejects_delivery_above_farmer_entitlement()
    {
        var allocation = new FarmerAllocation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50m);

        allocation.MarkDelivered(40m);

        Assert.False(allocation.CanDeliver(11m));
        Assert.Throws<InvalidOperationException>(() => allocation.MarkDelivered(11m));
    }
}
