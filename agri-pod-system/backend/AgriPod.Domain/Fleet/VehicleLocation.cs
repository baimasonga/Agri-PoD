using AgriPod.Domain.Common;

namespace AgriPod.Domain.Fleet;

public sealed class VehicleLocation : Entity
{
    private VehicleLocation()
    {
    }

    public VehicleLocation(string vehicleRegistration, decimal latitude, decimal longitude, decimal? speedKph, DateTimeOffset recordedAt)
    {
        VehicleRegistration = vehicleRegistration.Trim().ToUpperInvariant();
        Latitude = latitude;
        Longitude = longitude;
        SpeedKph = speedKph;
        RecordedAt = recordedAt;
    }

    public string VehicleRegistration { get; private set; } = "";
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public decimal? SpeedKph { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
}
