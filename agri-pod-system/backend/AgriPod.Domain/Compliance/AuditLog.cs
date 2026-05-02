using AgriPod.Domain.Common;

namespace AgriPod.Domain.Compliance;

public sealed class AuditLog : Entity
{
    private AuditLog()
    {
    }

    public AuditLog(string actorUserId, string action, string entityType, Guid? entityId, string metadataJson)
    {
        ActorUserId = actorUserId.Trim();
        Action = action.Trim();
        EntityType = entityType.Trim();
        EntityId = entityId;
        MetadataJson = metadataJson;
    }

    public string ActorUserId { get; private set; } = "";
    public string Action { get; private set; } = "";
    public string EntityType { get; private set; } = "";
    public Guid? EntityId { get; private set; }
    public string MetadataJson { get; private set; } = "";
}
