using AgriPod.Shared;
using AgriPod.Application.Abstractions;
using AgriPod.Domain.Common;
using AgriPod.Domain.Compliance;
using AgriPod.Domain.Deliveries;
using AgriPod.Domain.Dispatch;
using AgriPod.Domain.Farmers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Sync;

public sealed class SyncService(IAgriPodDbContext db)
{
    public async Task<SyncResponse> ApplyAsync(SyncRequest request, CancellationToken cancellationToken)
    {
        var accepted = new List<Guid>();

        foreach (var mutation in request.Mutations)
        {
            if (mutation.EntityName.Equals("Farmer", StringComparison.OrdinalIgnoreCase) &&
                mutation.Operation.Equals("Register", StringComparison.OrdinalIgnoreCase))
            {
                var farmerRequest = JsonSerializer.Deserialize<RegisterFarmerRequest>(
                    mutation.JsonPayload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (farmerRequest is not null)
                {
                    var farmer = new Farmer(
                        farmerRequest.FullName,
                        farmerRequest.NationalId,
                        farmerRequest.Phone,
                        farmerRequest.DistrictCode,
                        farmerRequest.Chiefdom,
                        farmerRequest.Community,
                        farmerRequest.ValueChain,
                        farmerRequest.Latitude,
                        farmerRequest.Longitude,
                        farmerRequest.PhotoReference);

                    db.Farmers.Add(farmer);
                    db.AuditLogs.Add(new(request.DeviceId, "OfflineFarmerRegistered", nameof(Farmer), farmer.Id, mutation.JsonPayload));
                    accepted.Add(mutation.ClientMutationId);
                }
            }
            else if (mutation.EntityName.Equals("DistributionSession", StringComparison.OrdinalIgnoreCase) &&
                     mutation.Operation.Equals("Start", StringComparison.OrdinalIgnoreCase))
            {
                var payload = JsonSerializer.Deserialize<DistributionSessionSyncPayload>(
                    mutation.JsonPayload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (payload is not null)
                {
                    var dispatch = await db.VehicleDispatches.FirstOrDefaultAsync(
                        x => x.ManifestBarcode == payload.ManifestBarcode,
                        cancellationToken);

                    if (dispatch is null)
                    {
                        db.ExceptionCases.Add(new ExceptionCase("ManifestNotFound", "High", $"Manifest {payload.ManifestBarcode} was not found during offline session sync.", "DistributionSession", Guid.Empty));
                    }
                    else
                    {
                        var session = new DistributionSession(dispatch.CampaignId, dispatch.Id, payload.FieldOfficerUserId, payload.Latitude, payload.Longitude);
                        db.DistributionSessions.Add(session);
                        db.AuditLogs.Add(new(request.DeviceId, "OfflineDistributionSessionStarted", nameof(DistributionSession), session.Id, mutation.JsonPayload));
                    }

                    accepted.Add(mutation.ClientMutationId);
                }
            }
            else if (mutation.EntityName.Equals("ProofOfDelivery", StringComparison.OrdinalIgnoreCase) &&
                     mutation.Operation.Equals("Confirm", StringComparison.OrdinalIgnoreCase))
            {
                var payload = JsonSerializer.Deserialize<ProofOfDeliverySyncPayload>(
                    mutation.JsonPayload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (payload is not null)
                {
                    await ApplyProofOfDeliveryAsync(request.DeviceId, payload, mutation.JsonPayload, cancellationToken);
                    accepted.Add(mutation.ClientMutationId);
                }
            }
            else
            {
                accepted.Add(mutation.ClientMutationId);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        var response = new SyncResponse(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), [], accepted);
        return response;
    }

    private async Task ApplyProofOfDeliveryAsync(string deviceId, ProofOfDeliverySyncPayload payload, string jsonPayload, CancellationToken cancellationToken)
    {
        // 1. Idempotency
        var alreadyProcessed = await db.Deliveries
            .AnyAsync(x => x.ProofEvents.Any(p => p.OfflineTransactionId == payload.OfflineTransactionId), cancellationToken);

        if (alreadyProcessed) return;

        var messages = new List<string>();
        var farmer = await db.Farmers.FirstOrDefaultAsync(x => x.BarcodeToken == payload.FarmerBarcode, cancellationToken);
        var allocation = farmer is null
            ? null
            : await db.FarmerAllocations.FirstOrDefaultAsync(x => x.FarmerId == farmer.Id, cancellationToken);

        var campaign = allocation is not null ? await db.Campaigns.FirstOrDefaultAsync(x => x.Id == allocation.CampaignId, cancellationToken) : null;
        var deliveries = await db.Deliveries.Include(x => x.Lines).Include(x => x.ProofEvents).ToListAsync(cancellationToken);
        var delivery = deliveries.FirstOrDefault(x => x.Lines.Any(line => line.Barcode == payload.PackageBarcode));
        var duplicatePackageDelivery = deliveries.Any(x =>
            x.Id != delivery?.Id &&
            x.Status == DeliveryStatus.Delivered &&
            x.Lines.Any(line => line.Barcode == payload.PackageBarcode));
        var deliveryLine = delivery?.Lines.FirstOrDefault(x => x.Barcode == payload.PackageBarcode);

        if (farmer is null) messages.Add("Farmer barcode not found.");
        if (allocation is null || !allocation.CanDeliver(payload.Quantity)) messages.Add("Allocation missing or quantity exceeds entitlement.");
        if (farmer is not null && allocation is not null && allocation.FarmerId != farmer.Id) messages.Add("Farmer barcode does not match allocation.");
        if (delivery is null) messages.Add("Package barcode was not found on a delivery.");
        if (delivery is not null && delivery.Status == DeliveryStatus.Delivered) messages.Add("Package has already been delivered.");
        if (deliveryLine is not null && allocation is not null && deliveryLine.InventoryItemId != allocation.InventoryItemId) messages.Add("Package item does not match farmer allocation.");
        if (deliveryLine is not null && deliveryLine.Quantity < payload.Quantity) messages.Add("Requested delivery quantity exceeds package quantity.");
        if (duplicatePackageDelivery) messages.Add("Duplicate package delivery detected.");
        if (campaign is null) messages.Add("Campaign not found.");
        if (string.IsNullOrWhiteSpace(payload.OtpCode)) messages.Add("OTP is required.");
        if (string.IsNullOrWhiteSpace(payload.FaceCaptureReference)) messages.Add("Face capture reference is required.");
        if (string.IsNullOrWhiteSpace(payload.PhotoEvidenceReference)) messages.Add("Photo evidence is required.");

        var vehiclePing = await db.VehicleLocations
            .Where(x => x.VehicleRegistration == payload.VehicleRegistration)
            .ToListAsync(cancellationToken);

        var latestVehiclePing = vehiclePing.OrderByDescending(x => x.RecordedAt).FirstOrDefault();
        if (latestVehiclePing is null) messages.Add("Vehicle GPS proximity could not be validated.");

        // 2. GPS Geofencing
        if (campaign is not null)
        {
            var distance = GpsDistanceCalculator.CalculateDistance(payload.Latitude, payload.Longitude, campaign.SiteLatitude, campaign.SiteLongitude);
            if (distance > (double)campaign.GpsRadiusMeters)
            {
                messages.Add($"GPS Geofence Breach: Delivery attempt at {distance:F0}m from site (Max: {campaign.GpsRadiusMeters}m).");
            }
        }

        if (latestVehiclePing is not null)
        {
            var vehicleDistance = GpsDistanceCalculator.CalculateDistance(payload.Latitude, payload.Longitude, latestVehiclePing.Latitude, latestVehiclePing.Longitude);
            if (vehicleDistance > 250)
            {
                messages.Add($"Vehicle GPS proximity mismatch: vehicle is {vehicleDistance:F0}m from delivery capture point.");
            }

            if (payload.Timestamp - latestVehiclePing.RecordedAt > TimeSpan.FromHours(2))
            {
                messages.Add("Vehicle GPS ping is too old for PoD confirmation.");
            }
        }

        // 3. Inventory Availability
        var stockLot = allocation is not null 
            ? await db.StockLots.FirstOrDefaultAsync(x => x.ItemId == allocation.InventoryItemId && x.Quantity >= payload.Quantity, cancellationToken)
            : null;
        
        if (stockLot is null)
        {
            messages.Add("Insufficient inventory in stock lots for this item.");
        }

        if (messages.Count > 0)
        {
            db.ExceptionCases.Add(new ExceptionCase("OfflinePoDValidationFailure", "High", string.Join(" ", messages), "ProofOfDelivery", delivery?.Id ?? Guid.Empty));
            db.AuditLogs.Add(new(deviceId, "OfflineProofOfDeliveryFlagged", "ProofOfDelivery", delivery?.Id, jsonPayload));
            return;
        }

        allocation!.MarkDelivered(payload.Quantity);
        stockLot!.Adjust(-payload.Quantity);
        delivery!.MarkDelivered(
            payload.Latitude,
            payload.Longitude,
            payload.CapturedByUserId,
            payload.FaceCaptureReference,
            payload.Timestamp,
            payload.SignatureReference,
            payload.PhotoEvidenceReference,
            payload.OfflineTransactionId);
        db.AuditLogs.Add(new(deviceId, "OfflineProofOfDeliveryConfirmed", "Delivery", delivery.Id, jsonPayload));
    }

    private sealed record DistributionSessionSyncPayload(string ManifestBarcode, string FieldOfficerUserId, decimal Latitude, decimal Longitude);

    private sealed record ProofOfDeliverySyncPayload(
        string FarmerBarcode,
        string PackageBarcode,
        string OtpCode,
        string FaceCaptureReference,
        decimal Latitude,
        decimal Longitude,
        string VehicleRegistration,
        decimal Quantity,
        string CapturedByUserId,
        DateTimeOffset Timestamp,
        string? SignatureReference,
        string? PhotoEvidenceReference,
        string OfflineTransactionId);
}
