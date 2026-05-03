using AgriPod.Domain.Common;

namespace AgriPod.Domain.Traceability;

public sealed class BarcodeToken : Entity
{
    private BarcodeToken()
    {
    }

    public BarcodeToken(string tokenType, string token, string entityReference, string generatedByUserId)
    {
        TokenType = tokenType.Trim().ToUpperInvariant();
        Token = token.Trim().ToUpperInvariant();
        EntityReference = entityReference.Trim();
        GeneratedByUserId = generatedByUserId.Trim();
        GeneratedAt = DateTimeOffset.UtcNow;
    }

    public string TokenType { get; private set; } = "";
    public string Token { get; private set; } = "";
    public string EntityReference { get; private set; } = "";
    public string GeneratedByUserId { get; private set; } = "";
    public DateTimeOffset GeneratedAt { get; private set; }
    public bool IsRevoked { get; private set; }

    public void Revoke()
    {
        IsRevoked = true;
        Touch();
    }
}
