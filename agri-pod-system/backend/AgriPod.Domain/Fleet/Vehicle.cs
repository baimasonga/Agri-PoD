using AgriPod.Domain.Common;

namespace AgriPod.Domain.Fleet;

public sealed class Vehicle : Entity
{
    private Vehicle()
    {
    }

    public Vehicle(string registration, string driverUserId, bool trackingActive)
    {
        Registration = registration.Trim().ToUpperInvariant();
        DriverUserId = driverUserId.Trim();
        TrackingActive = trackingActive;
    }

    public string Registration { get; private set; } = "";
    public string DriverUserId { get; private set; } = "";
    public bool TrackingActive { get; private set; }
}
