using AgriPod.Domain.Common;

namespace AgriPod.Domain.Traceability;

public sealed class StockReconciliation : Entity
{
    private StockReconciliation()
    {
    }

    public StockReconciliation(
        Guid campaignId,
        string campaignName,
        decimal loadedQuantity,
        decimal deliveredQuantity,
        decimal returnedQuantity,
        string supervisorUserId,
        string notes)
    {
        CampaignId = campaignId;
        CampaignName = campaignName.Trim();
        LoadedQuantity = loadedQuantity;
        DeliveredQuantity = deliveredQuantity;
        ReturnedQuantity = returnedQuantity;
        DiscrepancyQuantity = loadedQuantity - deliveredQuantity - returnedQuantity;
        Status = DiscrepancyQuantity == 0 ? "Balanced" : "Discrepancy";
        SupervisorUserId = supervisorUserId.Trim();
        Notes = notes.Trim();
        SubmittedAt = DateTimeOffset.UtcNow;
    }

    public Guid CampaignId { get; private set; }
    public string CampaignName { get; private set; } = "";
    public decimal LoadedQuantity { get; private set; }
    public decimal DeliveredQuantity { get; private set; }
    public decimal ReturnedQuantity { get; private set; }
    public decimal DiscrepancyQuantity { get; private set; }
    public string Status { get; private set; } = "";
    public string SupervisorUserId { get; private set; } = "";
    public string Notes { get; private set; } = "";
    public DateTimeOffset SubmittedAt { get; private set; }
    public string ReviewStatus { get; private set; } = "PendingReview";

    public void MarkReviewed(string decision)
    {
        ReviewStatus = decision.Trim();
        Touch();
    }
}
