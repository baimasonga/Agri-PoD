using AgriPod.Application.Abstractions;
using AgriPod.Domain.Dispatch;
using AgriPod.Domain.Fleet;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Dispatch;

public sealed class DispatchService(IAgriPodDbContext db)
{
    public async Task<IReadOnlyCollection<VehicleDto>> VehiclesAsync(CancellationToken cancellationToken) =>
        await db.Vehicles
            .OrderBy(x => x.Registration)
            .Select(x => new VehicleDto(x.Id, x.Registration, x.DriverUserId, x.TrackingActive))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<DispatchDto>> DispatchesAsync(CancellationToken cancellationToken) =>
        await db.VehicleDispatches
            .OrderBy(x => x.VehicleRegistration)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);

    public async Task<VehicleDto> RegisterVehicleAsync(RegisterVehicleRequest request, CancellationToken cancellationToken)
    {
        var vehicle = new Vehicle(request.Registration, request.DriverUserId, request.TrackingActive);
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(cancellationToken);
        return new VehicleDto(vehicle.Id, vehicle.Registration, vehicle.DriverUserId, vehicle.TrackingActive);
    }

    public async Task<DispatchDto> CreateDispatchAsync(CreateDispatchRequest request, CancellationToken cancellationToken)
    {
        var dispatch = new VehicleDispatch(request.CampaignId, request.WarehouseId, request.VehicleRegistration, request.DriverUserId);
        db.VehicleDispatches.Add(dispatch);
        db.AuditLogs.Add(new("warehouse-manager", "DispatchCreated", nameof(VehicleDispatch), dispatch.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(dispatch);
    }

    public async Task<DispatchDto?> ConfirmLoadedAsync(Guid id, CancellationToken cancellationToken)
    {
        var dispatch = await db.VehicleDispatches.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (dispatch is null)
        {
            return null;
        }

        dispatch.ConfirmLoaded();
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(dispatch);
    }

    public async Task<DistributionSessionDto?> StartSessionAsync(StartDistributionSessionRequest request, CancellationToken cancellationToken)
    {
        var dispatch = await db.VehicleDispatches.FirstOrDefaultAsync(x => x.Id == request.DispatchId, cancellationToken);
        if (dispatch is null || !dispatch.ManifestBarcode.Equals(request.ManifestBarcode, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var session = new DistributionSession(request.CampaignId, request.DispatchId, request.FieldOfficerUserId, request.Latitude, request.Longitude);
        db.DistributionSessions.Add(session);
        db.AuditLogs.Add(new(request.FieldOfficerUserId, "DistributionSessionStarted", nameof(DistributionSession), session.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return new DistributionSessionDto(session.Id, session.CampaignId, session.DispatchId, session.FieldOfficerUserId, session.Latitude, session.Longitude, session.StartedAt);
    }

    private static DispatchDto ToDto(VehicleDispatch dispatch) =>
        new(dispatch.Id, dispatch.CampaignId, dispatch.WarehouseId, dispatch.VehicleRegistration, dispatch.DriverUserId, dispatch.ManifestBarcode, dispatch.Status.ToString());
}
