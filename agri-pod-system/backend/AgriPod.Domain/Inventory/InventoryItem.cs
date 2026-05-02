using AgriPod.Domain.Common;

namespace AgriPod.Domain.Inventory;

public sealed class InventoryItem : Entity
{
    private InventoryItem()
    {
    }

    public InventoryItem(string sku, string name, string category, string unitOfMeasure)
    {
        Sku = sku.Trim().ToUpperInvariant();
        Name = name.Trim();
        Category = category.Trim();
        UnitOfMeasure = unitOfMeasure.Trim();
    }

    public string Sku { get; private set; } = "";
    public string Name { get; private set; } = "";
    public string Category { get; private set; } = "";
    public string UnitOfMeasure { get; private set; } = "";
}
