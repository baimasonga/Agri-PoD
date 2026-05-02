namespace AgriPod.Domain.Sync;

public sealed record SyncEnvelope(
    string DeviceId,
    string DistrictCode,
    long LastServerVersion,
    IReadOnlyCollection<SyncMutation> Mutations);

public sealed record SyncMutation(
    Guid ClientMutationId,
    string EntityName,
    string Operation,
    string JsonPayload,
    DateTimeOffset OccurredAt);
