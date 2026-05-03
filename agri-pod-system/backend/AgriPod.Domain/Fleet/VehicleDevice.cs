namespace AgriPod.Domain.Fleet;

public sealed class VehicleDevice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DeviceId { get; set; } = string.Empty;
    public string VehicleRegistration { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTimeOffset RegisteredAt { get; set; } = DateTimeOffset.UtcNow;
}
