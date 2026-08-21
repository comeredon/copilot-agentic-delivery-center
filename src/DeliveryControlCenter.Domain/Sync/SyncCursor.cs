using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Sync;

public enum SyncFeed
{
    Issues = 0,
    PullRequests = 1,
    Checks = 2,
    ProjectItems = 3,
}

/// <summary>
/// Durable per-feed position. Polling is the source of truth, so the cursor must survive
/// restarts; the ETag lets the next poll be conditional and cheap.
/// </summary>
public sealed class SyncCursor : Entity
{
    private SyncCursor()
    {
        ETag = null;
        LastError = null;
    }

    public required SyncFeed Feed { get; init; }

    public DateTimeOffset? LastSyncedAtUtc { get; private set; }

    public string? ETag { get; private set; }

    public int ConsecutiveFailures { get; private set; }

    public string? LastError { get; private set; }

    public static SyncCursor ForFeed(SyncFeed feed) => new() { Feed = feed };

    public void RecordSuccess(DateTimeOffset syncedAtUtc, string? etag)
    {
        LastSyncedAtUtc = syncedAtUtc;
        ETag = etag;
        ConsecutiveFailures = 0;
        LastError = null;
        Touch();
    }

    public void RecordFailure(string error)
    {
        ConsecutiveFailures++;
        LastError = error;
        Touch();
    }

    /// <summary>After repeated failure the feed is parked for human attention rather than retried forever.</summary>
    public bool IsDeadLettered => ConsecutiveFailures >= 5;
}
