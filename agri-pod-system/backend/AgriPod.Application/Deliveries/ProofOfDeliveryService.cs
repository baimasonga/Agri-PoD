using AgriPod.Domain.Compliance;
using AgriPod.Domain.Common;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Deliveries;

public sealed class ProofOfDeliveryService(IAgriPodDbContext db)
{
    public async Task<ProofOfDeliveryResult> ConfirmAsync(ProofOfDeliveryRequest request, CancellationToken cancellationToken)
    {
        // 1. Idempotency Check
        var alreadyProcessed = await db.Deliveries
            .AnyAsync(x => x.ProofEvents.Any(p => p.OfflineTransactionId == request.OfflineTransactionId), cancellationToken);
        
        if (alreadyProcessed)
        {
            return new ProofOfDeliveryResult(true, "AlreadyProcessed", []);
        }

        var farmer = await db.Farmers.FirstOrDefaultAsync(x => x.BarcodeToken == request.FarmerBarcode, cancellationToken);
        var allocation = await db.FarmerAllocations.FirstOrDefaultAsync(x => x.Id == request.AllocationId, cancellationToken);
        var delivery = await db.Deliveries.Include(x => x.ProofEvents).FirstOrDefaultAsync(x => x.Id == request.DeliveryId, cancellationToken);
        var campaign = allocation is not null ? await db.Campaigns.FirstOrDefaultAsync(x => x.Id == allocation.CampaignId, cancellationToken) : null;
        var vehiclePing = await db.VehicleLocations
            .Where(x => x.VehicleRegistration == request.VehicleRegistration)
            .OrderByDescending(x => x.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (farmer is null) messages.Add("Farmer barcode was not found.");
        if (allocation is null || !allocation.CanDeliver(request.Quantity)) messages.Add("Allocation is missing or quantity exceeds entitlement.");
        if (delivery is null) messages.Add("Delivery was not found.");
        if (campaign is null) messages.Add("Campaign was not found.");
        if (string.IsNullOrWhiteSpace(request.PackageBarcode)) messages.Add("Package barcode is required.");
        if (string.IsNullOrWhiteSpace(request.OtpCode)) messages.Add("OTP evidence is required.");
        if (string.IsNullOrWhiteSpace(request.FaceCaptureReference)) messages.Add("Face capture reference is required.");
        if (string.IsNullOrWhiteSpace(request.PhotoEvidenceReference)) messages.Add("Photo evidence is required.");
        if (vehiclePing is null) messages.Add("Vehicle GPS proximity could not be validated.");

        // 2. GPS Geofencing
        if (campaign is not null)
        {
            var distance = GpsDistanceCalculator.CalculateDistance(request.Latitude, request.Longitude, campaign.SiteLatitude, campaign.SiteLongitude);
            if (distance > (double)campaign.GpsRadiusMeters)
            {
                messages.Add($"GPS Geofence Breach: Delivery attempt at {distance:F0}m from site (Max: {campaign.GpsRadiusMeters}m).");
            }
        }

        // 3. Inventory Availability
        var stockLot = allocation is not null 
            ? await db.StockLots.FirstOrDefaultAsync(x => x.ItemId == allocation.InventoryItemId && x.Quantity >= request.Quantity, cancellationToken)
            : null;
        
        if (stockLot is null)
        {
            messages.Add("Insufficient inventory in stock lots for this item.");
        }

        if (messages.Count > 0)
        {
            var relatedId = delivery?.Id ?? allocation?.Id ?? Guid.Empty;
            db.ExceptionCases.Add(new ExceptionCase("PoDValidationFailure", "High", string.Join(" ", messages), "ProofOfDelivery", relatedId));
            await db.SaveChangesAsync(cancellationToken);
            return new ProofOfDeliveryResult(false, "Exception", messages);
        }

        // 4. Persistence & Stock Deduction
        allocation!.MarkDelivered(request.Quantity);
        stockLot!.Adjust(-request.Quantity);
        delivery!.MarkDelivered(
            request.Latitude,
            request.Longitude,
            request.CapturedByUserId,
            request.FaceCaptureReference,
            request.Timestamp,
            request.SignatureReference,
            request.PhotoEvidenceReference,
            request.OfflineTransactionId);
        
        db.AuditLogs.Add(new(request.CapturedByUserId, "ProofOfDeliveryConfirmed", "Delivery", delivery.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return new ProofOfDeliveryResult(true, "Delivered", []);
    }
}
