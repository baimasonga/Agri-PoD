using AgriPod.Application.Abstractions;
using AgriPod.Domain.Campaigns;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Campaigns;

public sealed class CampaignService(IAgriPodDbContext db)
{
    public async Task<IReadOnlyCollection<CampaignDto>> ListAsync(CancellationToken cancellationToken) =>
        (await db.Campaigns.ToListAsync(cancellationToken))
            .OrderByDescending(x => x.CreatedAt)
            .Select(ToDto)
            .ToList();

    public async Task<CampaignDto> CreateAsync(CreateCampaignRequest request, CancellationToken cancellationToken)
    {
        var campaign = new Campaign(request.Name, request.DistrictCode, request.ValueChain, request.StartsOn, request.EndsOn, request.SiteLatitude, request.SiteLongitude, request.GpsRadiusMeters);
        campaign.SubmitForApproval();
        db.Campaigns.Add(campaign);
        db.AuditLogs.Add(new("project-manager", "CampaignSubmitted", nameof(Campaign), campaign.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(campaign);
    }

    public async Task<CampaignDto?> ApproveAsync(Guid id, CancellationToken cancellationToken)
    {
        var campaign = await db.Campaigns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (campaign is null)
        {
            return null;
        }

        campaign.Approve();
        db.AuditLogs.Add(new("project-manager", "CampaignApproved", nameof(Campaign), campaign.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(campaign);
    }

    public async Task<AllocationDto> AllocateAsync(AllocateFarmerRequest request, CancellationToken cancellationToken)
    {
        var existing = await db.FarmerAllocations.FirstOrDefaultAsync(
            x => x.CampaignId == request.CampaignId && x.FarmerId == request.FarmerId && x.InventoryItemId == request.InventoryItemId,
            cancellationToken);

        if (existing is not null)
        {
            return new AllocationDto(existing.Id, existing.CampaignId, existing.FarmerId, existing.InventoryItemId, existing.AllocatedQuantity, existing.DeliveredQuantity, existing.AllocationToken);
        }

        var allocation = new FarmerAllocation(request.CampaignId, request.FarmerId, request.InventoryItemId, request.AllocatedQuantity);
        db.FarmerAllocations.Add(allocation);
        db.AuditLogs.Add(new("project-manager", "FarmerAllocated", nameof(FarmerAllocation), allocation.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return new AllocationDto(allocation.Id, allocation.CampaignId, allocation.FarmerId, allocation.InventoryItemId, allocation.AllocatedQuantity, allocation.DeliveredQuantity, allocation.AllocationToken);
    }

    public async Task<IReadOnlyCollection<AllocationDto>> AllocationsAsync(Guid campaignId, CancellationToken cancellationToken) =>
        await db.FarmerAllocations
            .Where(x => x.CampaignId == campaignId)
            .Select(x => new AllocationDto(x.Id, x.CampaignId, x.FarmerId, x.InventoryItemId, x.AllocatedQuantity, x.DeliveredQuantity, x.AllocationToken))
            .ToListAsync(cancellationToken);

    private static CampaignDto ToDto(Campaign campaign) =>
        new(campaign.Id, campaign.Name, campaign.DistrictCode, campaign.ValueChain, campaign.StartsOn, campaign.EndsOn, campaign.SiteLatitude, campaign.SiteLongitude, campaign.GpsRadiusMeters, campaign.Status.ToString());
}
