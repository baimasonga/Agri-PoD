using AgriPod.Domain.Common;

namespace AgriPod.Domain.Dispatch;

public sealed class DistributionSession : Entity
{
    private DistributionSession()
    {
    }

    public DistributionSession(Guid campaignId, Guid dispatchId, string fieldOfficerUserId, decimal latitude, decimal longitude)
    {
        CampaignId = campaignId;
        DispatchId = dispatchId;
        FieldOfficerUserId = fieldOfficerUserId.Trim();
        Latitude = latitude;
        Longitude = longitude;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public Guid CampaignId { get; private set; }
    public Guid DispatchId { get; private set; }
    public string FieldOfficerUserId { get; private set; } = "";
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
}
