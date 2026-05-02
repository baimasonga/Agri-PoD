namespace AgriPod.Blazor.Security;

public sealed class PortalAuthState
{
    public string? AccessToken { get; private set; }
    public string? Email { get; private set; }
    public string? Role { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken) && ExpiresAt > DateTimeOffset.UtcNow;

    public void SignIn(string email, string role, string accessToken, DateTimeOffset expiresAt)
    {
        Email = email;
        Role = role;
        AccessToken = accessToken;
        ExpiresAt = expiresAt;
    }

    public void SignOut()
    {
        AccessToken = null;
        Email = null;
        Role = null;
        ExpiresAt = null;
    }

    public bool IsInRole(params string[] roles) =>
        IsAuthenticated && Role is not null && roles.Any(role => role.Equals(Role, StringComparison.OrdinalIgnoreCase));
}
