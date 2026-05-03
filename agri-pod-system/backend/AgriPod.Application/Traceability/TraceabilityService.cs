using AgriPod.Application.Abstractions;
using AgriPod.Domain.Compliance;
using AgriPod.Domain.Traceability;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Traceability;

public sealed class TraceabilityService(IAgriPodDbContext db)
{
    private static readonly HashSet<string> AllowedTokenTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "FARMER",
        "PACKAGE",
        "MANIFEST"
    };

    public async Task<IReadOnlyCollection<BarcodeTokenDto>> BarcodeTokensAsync(CancellationToken cancellationToken)
    {
        var tokens = await db.BarcodeTokens.ToListAsync(cancellationToken);
        return tokens
            .OrderByDescending(x => x.GeneratedAt)
            .Take(50)
            .Select(ToDto)
            .ToList();
    }

    public async Task<BarcodeTokenDto> GenerateBarcodeAsync(CreateBarcodeTokenRequest request, CancellationToken cancellationToken)
    {
        var tokenType = request.TokenType.Trim().ToUpperInvariant();
        if (!AllowedTokenTypes.Contains(tokenType))
        {
            throw new InvalidOperationException("Barcode token type must be FARMER, PACKAGE, or MANIFEST.");
        }

        if (string.IsNullOrWhiteSpace(request.EntityReference))
        {
            throw new InvalidOperationException("Barcode token requires an entity reference.");
        }

        var token = $"{tokenType}-{Guid.NewGuid():N}";
        var barcode = new BarcodeToken(tokenType, token, request.EntityReference, request.GeneratedByUserId);
        db.BarcodeTokens.Add(barcode);
        db.AuditLogs.Add(new(barcode.GeneratedByUserId, "BarcodeTokenGenerated", nameof(BarcodeToken), barcode.Id, $"{{\"token\":\"{barcode.Token}\",\"entityReference\":\"{barcode.EntityReference}\"}}"));
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(barcode);
    }

    public async Task<IReadOnlyCollection<StockReconciliationDto>> StockReconciliationsAsync(CancellationToken cancellationToken)
    {
        var reconciliations = await db.StockReconciliations.ToListAsync(cancellationToken);
        return reconciliations
            .OrderByDescending(x => x.SubmittedAt)
            .Take(50)
            .Select(ToDto)
            .ToList();
    }

    public async Task<StockReconciliationDto> PreviewReconciliationAsync(Guid campaignId, decimal returnedQuantity, CancellationToken cancellationToken)
    {
        if (returnedQuantity < 0)
        {
            throw new InvalidOperationException("Returned quantity cannot be negative.");
        }

        var campaign = await db.Campaigns.FirstAsync(x => x.Id == campaignId, cancellationToken);
        var allocations = await db.FarmerAllocations.Where(x => x.CampaignId == campaignId).ToListAsync(cancellationToken);
        var dispatches = await db.VehicleDispatches.Where(x => x.CampaignId == campaignId).ToListAsync(cancellationToken);

        var loadedQuantity = allocations.Sum(x => x.AllocatedQuantity);
        var deliveredQuantity = allocations.Sum(x => x.DeliveredQuantity);
        var discrepancy = loadedQuantity - deliveredQuantity - returnedQuantity;
        var status = discrepancy == 0 ? "Balanced" : "Discrepancy";

        if (dispatches.Count == 0)
        {
            status = "NoDispatch";
        }

        return new StockReconciliationDto(
            Guid.NewGuid(),
            campaign.Id,
            campaign.Name,
            loadedQuantity,
            deliveredQuantity,
            returnedQuantity,
            discrepancy,
            status,
            DateTimeOffset.UtcNow);
    }

    public async Task<StockReconciliationDto> SubmitReconciliationAsync(CreateStockReconciliationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SupervisorUserId))
        {
            throw new InvalidOperationException("Reconciliation requires a supervisor user.");
        }

        var preview = await PreviewReconciliationAsync(request.CampaignId, request.ReturnedQuantity, cancellationToken);
        var reconciliation = new StockReconciliation(
            preview.CampaignId,
            preview.CampaignName,
            preview.LoadedQuantity,
            preview.DeliveredQuantity,
            preview.ReturnedQuantity,
            request.SupervisorUserId,
            request.Notes);

        db.StockReconciliations.Add(reconciliation);
        db.AuditLogs.Add(new(request.SupervisorUserId, "StockReconciliationSubmitted", nameof(StockReconciliation), reconciliation.Id, $"{{\"status\":\"{reconciliation.Status}\",\"discrepancy\":{reconciliation.DiscrepancyQuantity},\"notes\":\"{request.Notes}\"}}"));

        if (reconciliation.Status == "Discrepancy")
        {
            db.ExceptionCases.Add(new ExceptionCase(
                "StockDiscrepancy",
                "High",
                $"Campaign {reconciliation.CampaignName} reconciliation has discrepancy of {reconciliation.DiscrepancyQuantity:N2}. {request.Notes}",
                "Campaign",
                reconciliation.CampaignId));
        }

        await db.SaveChangesAsync(cancellationToken);
        return ToDto(reconciliation);
    }

    private static BarcodeTokenDto ToDto(BarcodeToken barcode) =>
        new(barcode.TokenType, barcode.Token, barcode.EntityReference, barcode.GeneratedAt, barcode.GeneratedByUserId, barcode.IsRevoked);

    private static StockReconciliationDto ToDto(StockReconciliation reconciliation) =>
        new(
            reconciliation.Id,
            reconciliation.CampaignId,
            reconciliation.CampaignName,
            reconciliation.LoadedQuantity,
            reconciliation.DeliveredQuantity,
            reconciliation.ReturnedQuantity,
            reconciliation.DiscrepancyQuantity,
            reconciliation.Status,
            reconciliation.SubmittedAt,
            reconciliation.SupervisorUserId,
            reconciliation.Notes,
            reconciliation.ReviewStatus);
}
