using DeliveryControlCenter.Domain.Delivery;

namespace DeliveryControlCenter.Application.Abstractions;

/// <summary>
/// The read-only boundary to GitHub. Implementations must never expose tokens, never accept
/// arbitrary URLs, and never call merge endpoints.
/// </summary>
public interface IGitHubReader
{
    Task<RemoteSyncResult> FetchIssuesAsync(string? etag, CancellationToken cancellationToken);

    Task<RemoteSyncResult> FetchPullRequestsAsync(string? etag, CancellationToken cancellationToken);
}

/// <summary>
/// A conditional-request outcome. <see cref="NotModified"/> means the ETag matched and no
/// items were transferred, which is the common case during steady-state polling.
/// </summary>
public sealed record RemoteSyncResult(
    bool NotModified,
    string? ETag,
    IReadOnlyList<RemoteWorkItem> Items)
{
    public static RemoteSyncResult Unchanged(string? etag) => new(true, etag, []);
}

public sealed record RemoteWorkItem(
    int Number,
    WorkItemKind Kind,
    string Title,
    string Url,
    WorkItemState State);

/// <summary>Guarded Project field writes. Disabled by default; every call is audited.</summary>
public interface IProjectWriter
{
    bool Enabled { get; }

    Task UpdateEvidenceFieldAsync(int workItemNumber, string evidenceValue, CancellationToken cancellationToken);
}

/// <summary>Pushes change notifications to connected clients after state is committed.</summary>
public interface IDeliveryNotifier
{
    Task NotifyAsync(string kind, Guid? workItemId, CancellationToken cancellationToken);
}

/// <summary>Cache-aside reads with explicit invalidation and observable counters.</summary>
public interface IDeliveryCache
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan localExpiration,
        TimeSpan distributedExpiration,
        CancellationToken cancellationToken);

    Task InvalidateAsync(string key, CancellationToken cancellationToken);

    CacheCounters Snapshot();
}

public sealed record CacheCounters(long Hits, long Misses, long Invalidations, long Errors)
{
    public double HitRatio => Hits + Misses == 0 ? 0d : (double)Hits / (Hits + Misses);
}
