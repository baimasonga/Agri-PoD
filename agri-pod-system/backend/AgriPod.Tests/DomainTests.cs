using AgriPod.Domain.Deliveries;
using AgriPod.Domain.Campaigns;
using AgriPod.Domain.Inventory;
using AgriPod.Domain.Administration;
using AgriPod.Domain.Traceability;

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

    [Fact]
    public void Device_binding_normalizes_device_district_and_email()
    {
        var binding = new DeviceBinding(" field-001 ", " bombali ", " Field@AgriPod.Local ");

        Assert.Equal("FIELD-001", binding.DeviceId);
        Assert.Equal("BOMBALI", binding.DistrictCode);
        Assert.Equal("field@agripod.local", binding.BoundUserEmail);
        Assert.Equal("Bound", binding.Status);
    }

    [Fact]
    public void Barcode_token_normalizes_traceability_identifier()
    {
        var token = new BarcodeToken(" package ", " package-abc123 ", " PKG-RICE-0001 ", "warehouse@agripod.local");

        Assert.Equal("PACKAGE", token.TokenType);
        Assert.Equal("PACKAGE-ABC123", token.Token);
        Assert.Equal("PKG-RICE-0001", token.EntityReference);
        Assert.False(token.IsRevoked);
    }

    [Fact]
    public void Stock_reconciliation_detects_discrepancy_and_starts_review()
    {
        var reconciliation = new StockReconciliation(
            Guid.NewGuid(),
            "Wet season seed support",
            100m,
            80m,
            10m,
            "coordinator@agripod.local",
            "Returned to warehouse.");

        Assert.Equal(10m, reconciliation.DiscrepancyQuantity);
        Assert.Equal("Discrepancy", reconciliation.Status);
        Assert.Equal("PendingReview", reconciliation.ReviewStatus);
    }
}
