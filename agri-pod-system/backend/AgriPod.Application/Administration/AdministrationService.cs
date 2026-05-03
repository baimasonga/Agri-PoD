using AgriPod.Application.Abstractions;
using AgriPod.Domain.Administration;
using AgriPod.Domain.Compliance;
using AgriPod.Shared;
using Microsoft.EntityFrameworkCore;

namespace AgriPod.Application.Administration;

public sealed class AdministrationService(IAgriPodDbContext db)
{
    public async Task<IReadOnlyCollection<DistrictDto>> DistrictsAsync(CancellationToken cancellationToken) =>
        await db.Districts
            .OrderBy(x => x.Name)
            .Select(x => new DistrictDto(x.Id, x.Code, x.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<ChiefdomDto>> ChiefdomsAsync(CancellationToken cancellationToken) =>
        await db.Chiefdoms
            .OrderBy(x => x.DistrictCode)
            .ThenBy(x => x.Name)
            .Select(x => new ChiefdomDto(x.Id, x.DistrictCode, x.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<DeviceBindingDto>> DeviceBindingsAsync(CancellationToken cancellationToken)
    {
        var bindings = await db.DeviceBindings.ToListAsync(cancellationToken);
        return bindings
            .OrderByDescending(x => x.BoundAt)
            .Take(50)
            .Select(x => new DeviceBindingDto(x.DeviceId, x.DistrictCode, x.BoundUserEmail, x.Status, x.BoundAt))
            .ToList();
    }

    public async Task<DistrictDto> CreateDistrictAsync(CreateDistrictRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var existing = await db.Districts.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
        if (existing is not null)
        {
            return new DistrictDto(existing.Id, existing.Code, existing.Name);
        }

        var district = new District(request.Code, request.Name);
        db.Districts.Add(district);
        db.AuditLogs.Add(new("admin@agripod.local", "DistrictCreated", nameof(District), district.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return new DistrictDto(district.Id, district.Code, district.Name);
    }

    public async Task<ChiefdomDto> CreateChiefdomAsync(CreateChiefdomRequest request, CancellationToken cancellationToken)
    {
        var districtCode = request.DistrictCode.Trim().ToUpperInvariant();
        if (!await db.Districts.AnyAsync(x => x.Code == districtCode, cancellationToken))
        {
            throw new InvalidOperationException("Chiefdom requires an existing district.");
        }

        var existing = await db.Chiefdoms.FirstOrDefaultAsync(
            x => x.DistrictCode == districtCode && x.Name == request.Name.Trim(),
            cancellationToken);
        if (existing is not null)
        {
            return new ChiefdomDto(existing.Id, existing.DistrictCode, existing.Name);
        }

        var chiefdom = new Chiefdom(request.DistrictCode, request.Name);
        db.Chiefdoms.Add(chiefdom);
        db.AuditLogs.Add(new("admin@agripod.local", "ChiefdomCreated", nameof(Chiefdom), chiefdom.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return new ChiefdomDto(chiefdom.Id, chiefdom.DistrictCode, chiefdom.Name);
    }

    public async Task<SystemSettingsDto> SettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        return ToDto(settings);
    }

    public async Task<SystemSettingsDto> UpdateSettingsAsync(UpdateSystemSettingsRequest request, CancellationToken cancellationToken)
    {
        if (request.OtpExpiryMinutes is < 1 or > 60)
        {
            throw new InvalidOperationException("OTP expiry must be between 1 and 60 minutes.");
        }

        if (request.DefaultGeofenceRadiusMeters is < 10 or > 5000)
        {
            throw new InvalidOperationException("Default geofence must be between 10 and 5000 meters.");
        }

        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        settings.Update(
            request.OtpExpiryMinutes,
            request.DefaultGeofenceRadiusMeters,
            request.DeviceBindingRequired,
            request.BiometricEvidenceRequired,
            request.OfflineSyncEnabled,
            request.SensitiveDataPolicy);

        db.AuditLogs.Add(new("admin@agripod.local", "SystemSettingsUpdated", nameof(SystemSetting), settings.Id, "{}"));
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(settings);
    }

    public async Task<DeviceBindingDto> BindDeviceAsync(RegisterDeviceBindingRequest request, CancellationToken cancellationToken)
    {
        var deviceId = request.DeviceId.Trim().ToUpperInvariant();
        var districtCode = request.DistrictCode.Trim().ToUpperInvariant();
        var email = request.BoundUserEmail.Trim().ToLowerInvariant();

        if (!await db.Districts.AnyAsync(x => x.Code == districtCode, cancellationToken))
        {
            throw new InvalidOperationException("Device binding requires an existing district.");
        }

        if (!await db.AppUsers.AnyAsync(x => x.Email == email && x.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("Device binding requires an active system user.");
        }

        var binding = await db.DeviceBindings.FirstOrDefaultAsync(x => x.DeviceId == deviceId, cancellationToken);
        if (binding is null)
        {
            binding = new DeviceBinding(deviceId, districtCode, email);
            db.DeviceBindings.Add(binding);
        }
        else
        {
            binding.Rebind(districtCode, email);
        }

        db.AuditLogs.Add(new(binding.BoundUserEmail, "DeviceBound", nameof(DeviceBinding), binding.Id, $"{{\"deviceId\":\"{binding.DeviceId}\",\"districtCode\":\"{binding.DistrictCode}\"}}"));
        await db.SaveChangesAsync(cancellationToken);
        return new DeviceBindingDto(binding.DeviceId, binding.DistrictCode, binding.BoundUserEmail, binding.Status, binding.BoundAt);
    }

    private async Task<SystemSetting> GetOrCreateSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await db.SystemSettings.FirstOrDefaultAsync(cancellationToken);
        if (settings is not null)
        {
            return settings;
        }

        settings = new SystemSetting(
            10,
            150m,
            true,
            true,
            true,
            "Biometric images and OTP proof are stored as secure references and audited on every PoD event.");
        db.SystemSettings.Add(settings);
        await db.SaveChangesAsync(cancellationToken);
        return settings;
    }

    private static SystemSettingsDto ToDto(SystemSetting settings) =>
        new(
            settings.OtpExpiryMinutes,
            settings.DefaultGeofenceRadiusMeters,
            settings.DeviceBindingRequired,
            settings.BiometricEvidenceRequired,
            settings.OfflineSyncEnabled,
            settings.SensitiveDataPolicy);
}
