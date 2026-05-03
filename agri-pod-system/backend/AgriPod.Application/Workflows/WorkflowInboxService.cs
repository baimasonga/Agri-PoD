using AgriPod.Application.Abstractions;
using AgriPod.Domain.Campaigns;
using AgriPod.Domain.Dispatch;
using AgriPod.Domain.Farmers;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Workflows;

public sealed class WorkflowInboxService(IAgriPodDbContext db)
{
    public async Task<WorkflowInboxDto> GetAsync(CancellationToken cancellationToken)
    {
        var pendingFarmers = await db.Farmers
            .Where(x => x.Status == FarmerStatus.PendingReview)
            .ToListAsync(cancellationToken);
        var pendingCampaigns = await db.Campaigns
            .Where(x => x.Status == CampaignStatus.PendingApproval)
            .ToListAsync(cancellationToken);
        var dispatchesAwaitingLoad = await db.VehicleDispatches
            .Where(x => x.Status == DispatchStatus.Loading)
            .ToListAsync(cancellationToken);
        var openExceptions = await db.ExceptionCases
            .Where(x => x.Status == "Open")
            .ToListAsync(cancellationToken);
        var pendingReconciliations = await db.StockReconciliations
            .Where(x => x.ReviewStatus == "PendingReview")
            .ToListAsync(cancellationToken);

        var tasks = new List<WorkflowTaskDto>();
        tasks.AddRange(pendingFarmers.Select(x => new WorkflowTaskDto(
            "Farmer approval",
            x.FullName,
            $"{x.DistrictCode} / {x.Chiefdom} / {x.ValueChain}",
            "Medium",
            "/farmers",
            x.CreatedAt)));
        tasks.AddRange(pendingCampaigns.Select(x => new WorkflowTaskDto(
            "Campaign approval",
            x.Name,
            $"{x.DistrictCode} / {x.ValueChain} / {x.StartsOn:yyyy-MM-dd}",
            "High",
            "/campaigns",
            x.CreatedAt)));
        tasks.AddRange(dispatchesAwaitingLoad.Select(x => new WorkflowTaskDto(
            "Dispatch loading",
            x.VehicleRegistration,
            $"Manifest {x.ManifestBarcode}",
            "High",
            "/fleet",
            x.CreatedAt)));
        tasks.AddRange(openExceptions.Select(x => new WorkflowTaskDto(
            x.CaseType,
            x.Severity,
            x.Description,
            x.Severity,
            "/compliance",
            x.CreatedAt)));
        tasks.AddRange(pendingReconciliations.Select(x => new WorkflowTaskDto(
            "Reconciliation review",
            x.CampaignName,
            $"{x.Status}: discrepancy {x.DiscrepancyQuantity:N0}",
            x.Status == "Discrepancy" ? "High" : "Medium",
            "/traceability",
            x.SubmittedAt)));

        return new WorkflowInboxDto(
            pendingFarmers.Count,
            pendingCampaigns.Count,
            dispatchesAwaitingLoad.Count,
            openExceptions.Count(x => x.CaseType.Contains("PoD", StringComparison.OrdinalIgnoreCase) || x.RelatedEntityType == "ProofOfDelivery"),
            openExceptions.Count(x => x.CaseType.Contains("Sync", StringComparison.OrdinalIgnoreCase) || x.CaseType.Contains("Offline", StringComparison.OrdinalIgnoreCase)),
            pendingReconciliations.Count,
            tasks
                .OrderByDescending(x => PriorityWeight(x.Priority))
                .ThenBy(x => x.CreatedAt)
                .Take(25)
                .ToList());
    }

    private static int PriorityWeight(string priority) => priority switch
    {
        "Critical" => 4,
        "High" => 3,
        "Medium" => 2,
        _ => 1
    };
}
