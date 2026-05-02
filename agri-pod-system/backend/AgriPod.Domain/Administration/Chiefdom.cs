using AgriPod.Domain.Common;

namespace AgriPod.Domain.Administration;

public sealed class Chiefdom : Entity
{
    private Chiefdom()
    {
    }

    public Chiefdom(string districtCode, string name)
    {
        DistrictCode = districtCode.Trim().ToUpperInvariant();
        Name = name.Trim();
    }

    public string DistrictCode { get; private set; } = "";
    public string Name { get; private set; } = "";
}
