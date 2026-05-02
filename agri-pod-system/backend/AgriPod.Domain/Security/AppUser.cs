using AgriPod.Domain.Common;

namespace AgriPod.Domain.Security;

public sealed class AppUser : Entity
{
    private AppUser()
    {
    }

    public AppUser(string fullName, string email, string phone, SystemRole role, string districtCode)
    {
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        Role = role;
        DistrictCode = districtCode.Trim().ToUpperInvariant();
    }

    public string FullName { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string Phone { get; private set; } = "";
    public SystemRole Role { get; private set; }
    public string DistrictCode { get; private set; } = "";
    public bool IsActive { get; private set; } = true;
}
