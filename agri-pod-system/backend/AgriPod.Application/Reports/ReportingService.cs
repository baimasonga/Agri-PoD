using AgriPod.Application.Abstractions;
using AgriPod.Domain.Campaigns;
using AgriPod.Domain.Deliveries;
using AgriPod.Domain.Farmers;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Reports;

public sealed class ReportingService(IAgriPodDbContext db)
{
    public async Task<ReportSummaryDto> SummaryAsync(CancellationToken cancellationToken)
    {
        var farmersRegistered = await db.Farmers.CountAsync(cancellationToken);
        var farmersApproved = await db.Farmers.CountAsync(x => x.Status == FarmerStatus.Approved, cancellationToken);
        var campaignsActive = await db.Campaigns.CountAsync(x => x.Status == CampaignStatus.Active || x.Status == CampaignStatus.Approved, cancellationToken);
        var deliveriesCompleted = await db.Deliveries.CountAsync(x => x.Status == DeliveryStatus.Delivered, cancellationToken);
        var openExceptions = await db.ExceptionCases.CountAsync(x => x.Status == "Open", cancellationToken);
        return new ReportSummaryDto(farmersRegistered, farmersApproved, campaignsActive, deliveriesCompleted, openExceptions, 0);
    }
}
