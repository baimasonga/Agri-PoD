namespace AgriPod.Domain.Deliveries;

public enum ProofEventType
{
    OtpVerified = 0,
    BarcodeScanned = 1,
    FaceCaptured = 2,
    Delivered = 3,
    Exception = 4
}
