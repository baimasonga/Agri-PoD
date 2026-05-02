using AgriPod.Domain.Common;

namespace AgriPod.Domain.Campaigns;

public sealed class Campaign : Entity
{
    private Campaign()
    {
    }

    public Campaign(string name, string districtCode, string valueChain, DateOnly startsOn, DateOnly endsOn, decimal siteLatitude, decimal siteLongitude, decimal gpsRadiusMeters)
    {
        Name = name.Trim();
        DistrictCode = districtCode.Trim().ToUpperInvariant();
        ValueChain = valueChain.Trim();
        StartsOn = startsOn;
        EndsOn = endsOn;
        SiteLatitude = siteLatitude;
        SiteLongitude = siteLongitude;
        GpsRadiusMeters = gpsRadiusMeters;
    }

    public string Name { get; private set; } = "";
    public string DistrictCode { get; private set; } = "";
    public string ValueChain { get; private set; } = "";
    public DateOnly StartsOn { get; private set; }
    public DateOnly EndsOn { get; private set; }
    public decimal SiteLatitude { get; private set; }
    public decimal SiteLongitude { get; private set; }
    public decimal GpsRadiusMeters { get; private set; }
    public CampaignStatus Status { get; private set; } = CampaignStatus.Draft;

    public void SubmitForApproval()
    {
        Status = CampaignStatus.PendingApproval;
        Touch();
    }

    public void Approve()
    {
        Status = CampaignStatus.Approved;
        Touch();
    }
}
