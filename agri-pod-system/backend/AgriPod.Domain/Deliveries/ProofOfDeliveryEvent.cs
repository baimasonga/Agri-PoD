using AgriPod.Domain.Common;

namespace AgriPod.Domain.Deliveries;

public sealed class ProofOfDeliveryEvent : Entity
{
    private ProofOfDeliveryEvent()
    {
    }

    public ProofOfDeliveryEvent(
        Guid deliveryId,
        ProofEventType type,
        decimal latitude,
        decimal longitude,
        string capturedByUserId,
        string? biometricReference,
        DateTimeOffset deliveredAt,
        string? signatureReference,
        string? photoEvidenceReference,
        string offlineTransactionId)
    {
        DeliveryId = deliveryId;
        Type = type;
        Latitude = latitude;
        Longitude = longitude;
        CapturedByUserId = capturedByUserId.Trim();
        BiometricReference = biometricReference?.Trim();
        DeliveredAt = deliveredAt;
        SignatureReference = signatureReference?.Trim();
        PhotoEvidenceReference = photoEvidenceReference?.Trim();
        OfflineTransactionId = offlineTransactionId.Trim();
    }

    public Guid DeliveryId { get; private set; }
    public ProofEventType Type { get; private set; }
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public string CapturedByUserId { get; private set; } = "";
    public string? BiometricReference { get; private set; }
    public DateTimeOffset DeliveredAt { get; private set; }
    public string? SignatureReference { get; private set; }
    public string? PhotoEvidenceReference { get; private set; }
    public string OfflineTransactionId { get; private set; } = "";
}
