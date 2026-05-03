using AgriPod.Domain.Common;

namespace AgriPod.Domain.Administration;

public sealed class SystemSetting : Entity
{
    private SystemSetting()
    {
    }

    public SystemSetting(
        int otpExpiryMinutes,
        decimal defaultGeofenceRadiusMeters,
        bool deviceBindingRequired,
        bool biometricEvidenceRequired,
        bool offlineSyncEnabled,
        string sensitiveDataPolicy)
    {
        OtpExpiryMinutes = otpExpiryMinutes;
        DefaultGeofenceRadiusMeters = defaultGeofenceRadiusMeters;
        DeviceBindingRequired = deviceBindingRequired;
        BiometricEvidenceRequired = biometricEvidenceRequired;
        OfflineSyncEnabled = offlineSyncEnabled;
        SensitiveDataPolicy = sensitiveDataPolicy.Trim();
    }

    public int OtpExpiryMinutes { get; private set; }
    public decimal DefaultGeofenceRadiusMeters { get; private set; }
    public bool DeviceBindingRequired { get; private set; }
    public bool BiometricEvidenceRequired { get; private set; }
    public bool OfflineSyncEnabled { get; private set; }
    public string SensitiveDataPolicy { get; private set; } = "";

    public void Update(
        int otpExpiryMinutes,
        decimal defaultGeofenceRadiusMeters,
        bool deviceBindingRequired,
        bool biometricEvidenceRequired,
        bool offlineSyncEnabled,
        string sensitiveDataPolicy)
    {
        OtpExpiryMinutes = otpExpiryMinutes;
        DefaultGeofenceRadiusMeters = defaultGeofenceRadiusMeters;
        DeviceBindingRequired = deviceBindingRequired;
        BiometricEvidenceRequired = biometricEvidenceRequired;
        OfflineSyncEnabled = offlineSyncEnabled;
        SensitiveDataPolicy = sensitiveDataPolicy.Trim();
        Touch();
    }
}
