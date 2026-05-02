using AgriPod.Domain.Common;

namespace AgriPod.Domain.Inventory;

public sealed class Warehouse : Entity
{
    private Warehouse()
    {
    }

    public Warehouse(string code, string name, string district)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        District = district.Trim();
    }

    public string Code { get; private set; } = "";
    public string Name { get; private set; } = "";
    public string District { get; private set; } = "";
}
