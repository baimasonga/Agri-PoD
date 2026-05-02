using AgriPod.Domain.Common;

namespace AgriPod.Domain.Dispatch;

public sealed class VehicleDispatch : Entity
{
    private VehicleDispatch()
    {
    }

    public VehicleDispatch(Guid campaignId, Guid warehouseId, string vehicleRegistration, string driverUserId)
    {
        CampaignId = campaignId;
        WarehouseId = warehouseId;
        VehicleRegistration = vehicleRegistration.Trim().ToUpperInvariant();
        DriverUserId = driverUserId.Trim();
        ManifestBarcode = $"MANIFEST-{Guid.NewGuid():N}";
    }

    public Guid CampaignId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string VehicleRegistration { get; private set; } = "";
    public string DriverUserId { get; private set; } = "";
    public string ManifestBarcode { get; private set; } = "";
    public DispatchStatus Status { get; private set; } = DispatchStatus.Loading;

    public void ConfirmLoaded()
    {
        Status = DispatchStatus.LoadedOnVehicle;
        Touch();
    }

    public void Dispatch()
    {
        Status = DispatchStatus.Dispatched;
        Touch();
    }
}
