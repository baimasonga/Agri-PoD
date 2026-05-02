using AgriPod.Domain.Common;

namespace AgriPod.Domain.Procurement;

public sealed class Supplier : Entity
{
    private Supplier()
    {
    }

    public Supplier(string name, string contactPhone)
    {
        Name = name.Trim();
        ContactPhone = contactPhone.Trim();
    }

    public string Name { get; private set; } = "";
    public string ContactPhone { get; private set; } = "";
}
