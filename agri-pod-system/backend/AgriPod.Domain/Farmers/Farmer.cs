using AgriPod.Domain.Common;

namespace AgriPod.Domain.Farmers;

public sealed class Farmer : Entity
{
    private Farmer()
    {
    }

    public Farmer(string fullName, string nationalId, string phone, string districtCode, string chiefdom, string community, string valueChain, decimal latitude, decimal longitude, string photoReference)
    {
        FullName = fullName.Trim();
        NationalId = nationalId.Trim();
        Phone = phone.Trim();
        DistrictCode = districtCode.Trim().ToUpperInvariant();
        Chiefdom = chiefdom.Trim();
        Community = community.Trim();
        ValueChain = valueChain.Trim();
        Latitude = latitude;
        Longitude = longitude;
        PhotoReference = photoReference.Trim();
        BarcodeToken = $"FARMER-{Guid.NewGuid():N}";
    }

    public string FullName { get; private set; } = "";
    public string NationalId { get; private set; } = "";
    public string Phone { get; private set; } = "";
    public string DistrictCode { get; private set; } = "";
    public string Chiefdom { get; private set; } = "";
    public string Community { get; private set; } = "";
    public string ValueChain { get; private set; } = "";
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public string PhotoReference { get; private set; } = "";
    public string BarcodeToken { get; private set; } = "";
    public FarmerStatus Status { get; private set; } = FarmerStatus.PendingReview;
    public string? ReviewComment { get; private set; }

    public void Review(bool approved, string? comment)
    {
        Status = approved ? FarmerStatus.Approved : FarmerStatus.Rejected;
        ReviewComment = comment?.Trim();
        Touch();
    }
}
