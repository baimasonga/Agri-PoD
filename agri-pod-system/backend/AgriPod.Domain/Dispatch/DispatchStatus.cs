namespace AgriPod.Domain.Dispatch;

public enum DispatchStatus
{
    Draft = 0,
    Loading = 1,
    LoadedOnVehicle = 2,
    Dispatched = 3,
    Arrived = 4,
    Reconciled = 5
}
