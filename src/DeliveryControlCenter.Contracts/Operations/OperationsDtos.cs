namespace DeliveryControlCenter.Contracts.Operations;

public sealed record SyncHealthDto(
    IReadOnlyList<SyncFeedHealthDto> Feeds,
    bool ProjectWritesEnabled);

public sealed record SyncFeedHealthDto(
    string Feed,
    DateTimeOffset? LastSyncedAtUtc,
    int ConsecutiveFailures,
    bool DeadLettered,
    string? LastError);

public sealed record CacheHealthDto(
    long Hits,
    long Misses,
    long Invalidations,
    long Errors,
    double HitRatio);

public sealed record AuditEventDto(
    DateTimeOffset OccurredAtUtc,
    string Actor,
    string Action,
    string Target,
    string Outcome,
    bool Denied);
