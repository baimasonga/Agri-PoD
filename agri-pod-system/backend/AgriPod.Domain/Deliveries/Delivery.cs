using AgriPod.Domain.Common;

namespace AgriPod.Domain.Deliveries;

public sealed class Delivery : Entity
{
    private readonly List<DeliveryLine> _lines = [];
    private readonly List<ProofOfDeliveryEvent> _proofEvents = [];

    private Delivery()
    {
    }

    public Delivery(string deliveryNumber, Guid originWarehouseId, string recipientName, string recipientPhone, string destinationDistrict)
    {
        DeliveryNumber = deliveryNumber.Trim().ToUpperInvariant();
        OriginWarehouseId = originWarehouseId;
        RecipientName = recipientName.Trim();
        RecipientPhone = recipientPhone.Trim();
        DestinationDistrict = destinationDistrict.Trim();
    }

    public string DeliveryNumber { get; private set; } = "";
    public Guid OriginWarehouseId { get; private set; }
    public string RecipientName { get; private set; } = "";
    public string RecipientPhone { get; private set; } = "";
    public string DestinationDistrict { get; private set; } = "";
    public DeliveryStatus Status { get; private set; } = DeliveryStatus.Draft;
    public IReadOnlyCollection<DeliveryLine> Lines => _lines;
    public IReadOnlyCollection<ProofOfDeliveryEvent> ProofEvents => _proofEvents;

    public void AddLine(Guid inventoryItemId, decimal quantity, string? barcode)
    {
        _lines.Add(new DeliveryLine(Id, inventoryItemId, quantity, barcode));
        Touch();
    }

    public void Dispatch()
    {
        Status = DeliveryStatus.InTransit;
        Touch();
    }

    public void MarkDelivered(
        decimal latitude,
        decimal longitude,
        string capturedByUserId,
        string? biometricReference,
        DateTimeOffset deliveredAt,
        string? signatureReference,
        string? photoEvidenceReference,
        string offlineTransactionId)
    {
        Status = DeliveryStatus.Delivered;
        _proofEvents.Add(new ProofOfDeliveryEvent(
            Id,
            ProofEventType.Delivered,
            latitude,
            longitude,
            capturedByUserId,
            biometricReference,
            deliveredAt,
            signatureReference,
            photoEvidenceReference,
            offlineTransactionId));
        Touch();
    }
}
