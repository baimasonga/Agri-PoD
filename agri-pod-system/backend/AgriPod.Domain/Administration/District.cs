using AgriPod.Domain.Common;

namespace AgriPod.Domain.Administration;

public sealed class District : Entity
{
    private District()
    {
    }

    public District(string code, string name)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
    }

    public string Code { get; private set; } = "";
    public string Name { get; private set; } = "";
}
