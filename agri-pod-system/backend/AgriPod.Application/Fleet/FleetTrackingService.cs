using AgriPod.Application.Abstractions;
using AgriPod.Domain.Campaigns;
using AgriPod.Domain.Fleet;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Fleet;

public sealed class FleetTrackingService(IAgriPodDbContext db)
{
    public async Task<VehicleDeviceDto> RegisterDeviceAsync(RegisterVehicleDeviceRequest request, CancellationToken cancellationToken)
    {
        var device = new VehicleDevice
        {
            DeviceId = request.DeviceId,
            VehicleRegistration = request.VehicleRegistration,
            SerialNumber = request.SerialNumber
        };

        db.VehicleDevices.Add(device);
        await db.SaveChangesAsync(cancellationToken);

        return new VehicleDeviceDto(device.Id, device.DeviceId, device.VehicleRegistration, device.SerialNumber, device.Status, device.RegisteredAt);
    }

    public async Task RecordLocationAsync(VehicleLocationRequest request, CancellationToken cancellationToken)
    {
        await RecordInternalAsync(request.VehicleRegistration, request.Latitude, request.Longitude, request.SpeedKph, request.RecordedAt, cancellationToken);
    }

    public async Task BulkRecordLocationAsync(BulkGpsPingRequest request, CancellationToken cancellationToken)
    {
        var device = await db.VehicleDevices.FirstOrDefaultAsync(x => x.DeviceId == request.DeviceId, cancellationToken);
        if (device == null) return;

        foreach (var ping in request.Pings)
        {
            await RecordInternalAsync(device.VehicleRegistration, ping.Latitude, ping.Longitude, ping.SpeedKph, ping.Timestamp, cancellationToken);
        }
    }

    private async Task RecordInternalAsync(string reg, decimal lat, decimal lon, decimal? speed, DateTimeOffset time, CancellationToken ct)
    {
        db.VehicleLocations.Add(new VehicleLocation(reg, lat, lon, speed, time));
        
        // --- Heuristic Detection (Simplified) ---
        
        // 1. Stop Detection: If speed is < 1kph for multiple pings (simplified to just checking speed here)
        if (speed < 1.0m)
        {
            // In a real system, we'd check previous pings to see if it's been stopped for > 5 mins
        }

        // 2. Geofence Validation: Check if within site boundary of active campaigns
        var activeCampaigns = await db.Campaigns.Where(x => x.Status == CampaignStatus.Approved).ToListAsync(ct);
        foreach (var campaign in activeCampaigns)
        {
            var distance = CalculateDistance(lat, lon, campaign.SiteLatitude, campaign.SiteLongitude);
            if (distance <= (double)campaign.GpsRadiusMeters)
            {
                // Vehicle has arrived at a distribution site
            }
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyCollection<VehicleLocationRequest>> LatestAsync(CancellationToken cancellationToken)
    {
        return (await db.VehicleLocations.ToListAsync(cancellationToken))
            .GroupBy(x => x.VehicleRegistration)
            .Select(group => group.OrderByDescending(x => x.RecordedAt).First())
            .OrderBy(x => x.VehicleRegistration)
            .Select(x => new VehicleLocationRequest(x.VehicleRegistration, x.Latitude, x.Longitude, x.SpeedKph, x.RecordedAt))
            .ToList();
    }

    private static double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        var r = 6371e3; // meters
        var phi1 = (double)lat1 * Math.PI / 180;
        var phi2 = (double)lat2 * Math.PI / 180;
        var dphi = (double)(lat2 - lat1) * Math.PI / 180;
        var dlambda = (double)(lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(dphi / 2) * Math.Sin(dphi / 2) +
                Math.Cos(phi1) * Math.Cos(phi2) *
                Math.Sin(dlambda / 2) * Math.Sin(dlambda / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return r * c;
    }
}
