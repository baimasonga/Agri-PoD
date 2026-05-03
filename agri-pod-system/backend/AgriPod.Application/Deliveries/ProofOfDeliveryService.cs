using AgriPod.Application.Abstractions;
using AgriPod.Domain.Compliance;
using AgriPod.Domain.Common;
using AgriPod.Domain.Deliveries;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Deliveries;

public sealed class ProofOfDeliveryService(IAgriPodDbContext db)
{
    public async Task<ProofOfDeliveryResult> ConfirmAsync(ProofOfDeliveryRequest request, CancellationToken cancellationToken)
    {
        var messages = new List<string>();

        // 1. Idempotency Check
        var alreadyProcessed = await db.Deliveries
            .AnyAsync(x => x.ProofEvents.Any(p => p.OfflineTransactionId == request.OfflineTransactionId), cancellationToken);
        
        if (alreadyProcessed)
        {
            return new ProofOfDeliveryResult(true, "AlreadyProcessed", []);
        }

        var farmer = await db.Farmers.FirstOrDefaultAsync(x => x.BarcodeToken == request.FarmerBarcode, cancellationToken);
        var allocation = await db.FarmerAllocations.FirstOrDefaultAsync(x => x.Id == request.AllocationId, cancellationToken);
        var packageBarcode = request.PackageBarcode.Trim();
        var delivery = await db.Deliveries
            .Include(x => x.Lines)
            .Include(x => x.ProofEvents)
            .FirstOrDefaultAsync(x => x.Id == request.DeliveryId, cancellationToken);
        var campaign = allocation is not null ? await db.Campaigns.FirstOrDefaultAsync(x => x.Id == allocation.CampaignId, cancellationToken) : null;
        var vehiclePings = await db.VehicleLocations
            .Where(x => x.VehicleRegistration == request.VehicleRegistration)
            .ToListAsync(cancellationToken);
        var vehiclePing = vehiclePings.OrderByDescending(x => x.RecordedAt).FirstOrDefault();
        var duplicatePackageDelivery = await db.Deliveries
            .Include(x => x.Lines)
            .AnyAsync(x => x.Id != request.DeliveryId &&
                x.Status == DeliveryStatus.Delivered &&
                x.Lines.Any(line => line.Barcode == packageBarcode),
                cancellationToken);
        var deliveryLine = delivery?.Lines.FirstOrDefault(x => x.Barcode == packageBarcode);

        if (farmer is null) messages.Add("Farmer barcode was not found.");
        if (allocation is null || !allocation.CanDeliver(request.Quantity)) messages.Add("Allocation is missing or quantity exceeds entitlement.");
        if (farmer is not null && allocation is not null && allocation.FarmerId != farmer.Id) messages.Add("Farmer barcode does not match the selected allocation.");
        if (delivery is null) messages.Add("Delivery was not found.");
        if (delivery is not null && delivery.Status == DeliveryStatus.Delivered) messages.Add("Package has already been delivered.");
        if (campaign is null) messages.Add("Campaign was not found.");
        if (string.IsNullOrWhiteSpace(request.PackageBarcode)) messages.Add("Package barcode is required.");
        if (!string.IsNullOrWhiteSpace(request.PackageBarcode) && deliveryLine is null) messages.Add("Package barcode is not assigned to this delivery.");
        if (deliveryLine is not null && allocation is not null && deliveryLine.InventoryItemId != allocation.InventoryItemId) messages.Add("Package item does not match farmer allocation.");
        if (deliveryLine is not null && deliveryLine.Quantity < request.Quantity) messages.Add("Requested delivery quantity exceeds package quantity.");
        if (duplicatePackageDelivery) messages.Add("Duplicate package delivery detected.");
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

        if (vehiclePing is not null)
        {
            var vehicleDistance = GpsDistanceCalculator.CalculateDistance(request.Latitude, request.Longitude, vehiclePing.Latitude, vehiclePing.Longitude);
            if (vehicleDistance > 250)
            {
                messages.Add($"Vehicle GPS proximity mismatch: vehicle is {vehicleDistance:F0}m from delivery capture point.");
            }

            if (request.Timestamp - vehiclePing.RecordedAt > TimeSpan.FromHours(2))
            {
                messages.Add("Vehicle GPS ping is too old for PoD confirmation.");
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
