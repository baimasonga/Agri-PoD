using AgriPod.Application.Abstractions;
using AgriPod.Domain.Fleet;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Fleet;

public sealed class FleetTrackingService(IAgriPodDbContext db)
{
    public async Task RecordLocationAsync(VehicleLocationRequest request, CancellationToken cancellationToken)
    {
        db.VehicleLocations.Add(new VehicleLocation(
            request.VehicleRegistration,
            request.Latitude,
            request.Longitude,
            request.SpeedKph,
            request.RecordedAt));

        await db.SaveChangesAsync(cancellationToken);
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
}
