using AgriPod.Domain.Common;

namespace AgriPod.Domain.Administration;

public sealed class DeviceBinding : Entity
{
    private DeviceBinding()
    {
    }

    public DeviceBinding(string deviceId, string districtCode, string boundUserEmail)
    {
        DeviceId = deviceId.Trim().ToUpperInvariant();
        DistrictCode = districtCode.Trim().ToUpperInvariant();
        BoundUserEmail = boundUserEmail.Trim().ToLowerInvariant();
        Status = "Bound";
        BoundAt = DateTimeOffset.UtcNow;
    }

    public string DeviceId { get; private set; } = "";
    public string DistrictCode { get; private set; } = "";
    public string BoundUserEmail { get; private set; } = "";
    public string Status { get; private set; } = "";
    public DateTimeOffset BoundAt { get; private set; }

    public void Rebind(string districtCode, string boundUserEmail)
    {
        DistrictCode = districtCode.Trim().ToUpperInvariant();
        BoundUserEmail = boundUserEmail.Trim().ToLowerInvariant();
        Status = "Bound";
        BoundAt = DateTimeOffset.UtcNow;
        Touch();
    }

    public void Suspend()
    {
        Status = "Suspended";
        Touch();
    }
}
