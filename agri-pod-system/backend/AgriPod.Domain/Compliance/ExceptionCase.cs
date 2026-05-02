using AgriPod.Domain.Common;

namespace AgriPod.Domain.Compliance;

public sealed class ExceptionCase : Entity
{
    private ExceptionCase()
    {
    }

    public ExceptionCase(string caseType, string severity, string description, string relatedEntityType, Guid relatedEntityId)
    {
        CaseType = caseType.Trim();
        Severity = severity.Trim();
        Description = description.Trim();
        RelatedEntityType = relatedEntityType.Trim();
        RelatedEntityId = relatedEntityId;
    }

    public string CaseType { get; private set; } = "";
    public string Severity { get; private set; } = "";
    public string Description { get; private set; } = "";
    public string RelatedEntityType { get; private set; } = "";
    public Guid RelatedEntityId { get; private set; }
    public string Status { get; private set; } = "Open";

    public void Resolve(string decision)
    {
        Status = decision.Trim();
        Touch();
    }
}
