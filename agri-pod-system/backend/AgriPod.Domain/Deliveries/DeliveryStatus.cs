namespace AgriPod.Domain.Deliveries;

public enum DeliveryStatus
{
    Draft = 0,
    Scheduled = 1,
    InTransit = 2,
    Delivered = 3,
    Exception = 4,
    SyncedFromOfflineNode = 5
}
