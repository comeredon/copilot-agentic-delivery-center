using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Domain.Audit;
using DeliveryControlCenter.Domain.Delivery;
using DeliveryControlCenter.Domain.Evidence;
using DeliveryControlCenter.Domain.Gates;
using DeliveryControlCenter.Domain.Sync;

namespace DeliveryControlCenter.UnitTests.Fakes;

internal sealed class FakeWorkItemRepository : IWorkItemRepository
{
    public List<WorkItem> Items { get; } = [];

    public Task<WorkItem?> FindByNumberAsync(int number, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(i => i.Number == number));

    public Task<WorkItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(i => i.Id == id));

    public Task<IReadOnlyList<WorkItem>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<WorkItem>>(Items);

    public Task AddAsync(WorkItem workItem, CancellationToken cancellationToken)
    {
        Items.Add(workItem);
        return Task.CompletedTask;
    }
}

internal sealed class FakeSyncCursorRepository : ISyncCursorRepository
{
    public List<SyncCursor> Cursors { get; } = [];

    public Task<SyncCursor> GetOrCreateAsync(SyncFeed feed, CancellationToken cancellationToken)
    {
        var cursor = Cursors.FirstOrDefault(c => c.Feed == feed);
        if (cursor is null)
        {
            cursor = SyncCursor.ForFeed(feed);
            Cursors.Add(cursor);
        }

        return Task.FromResult(cursor);
    }

    public Task<IReadOnlyList<SyncCursor>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<SyncCursor>>(Cursors);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.FromResult(0);
    }
}

internal sealed class FakeAuditLog : IAuditLog
{
    public List<AuditEvent> Events { get; } = [];

    public Task RecordAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        Events.Add(auditEvent);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditEvent>> ListRecentAsync(int take, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AuditEvent>>(Events.TakeLast(take).ToList());
}

internal sealed class FakeNotifier : IDeliveryNotifier
{
    public List<string> Notifications { get; } = [];

    public Task NotifyAsync(string kind, Guid? workItemId, CancellationToken cancellationToken)
    {
        Notifications.Add(kind);
        return Task.CompletedTask;
    }
}

/// <summary>Records invalidations so tests can assert that writes actually evict the read model.</summary>
internal sealed class FakeCache : IDeliveryCache
{
    private readonly Dictionary<string, object?> _entries = [];

    public List<string> Invalidated { get; } = [];

    public long Hits { get; private set; }

    public long Misses { get; private set; }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan localExpiration,
        TimeSpan distributedExpiration,
        CancellationToken cancellationToken)
    {
        if (_entries.TryGetValue(key, out var cached))
        {
            Hits++;
            return (T)cached!;
        }

        Misses++;
        var value = await factory(cancellationToken);
        _entries[key] = value;
        return value;
    }

    public Task InvalidateAsync(string key, CancellationToken cancellationToken)
    {
        Invalidated.Add(key);
        _entries.Remove(key);
        return Task.CompletedTask;
    }

    public CacheCounters Snapshot() => new(Hits, Misses, Invalidated.Count, 0);
}

internal sealed class FakeGitHubReader : IGitHubReader
{
    public string? CurrentETag { get; set; } = "\"v1\"";

    public List<RemoteWorkItem> Issues { get; } = [];

    public List<RemoteWorkItem> PullRequests { get; } = [];

    public int IssueCallCount { get; private set; }

    public bool ThrowOnFetch { get; set; }

    public Task<RemoteSyncResult> FetchIssuesAsync(string? etag, CancellationToken cancellationToken)
    {
        IssueCallCount++;

        if (ThrowOnFetch)
        {
            throw new HttpRequestException("simulated outage");
        }

        return Task.FromResult(
            string.Equals(etag, CurrentETag, StringComparison.Ordinal)
                ? RemoteSyncResult.Unchanged(CurrentETag)
                : new RemoteSyncResult(false, CurrentETag, Issues));
    }

    public Task<RemoteSyncResult> FetchPullRequestsAsync(string? etag, CancellationToken cancellationToken) =>
        Task.FromResult(
            string.Equals(etag, CurrentETag, StringComparison.Ordinal)
                ? RemoteSyncResult.Unchanged(CurrentETag)
                : new RemoteSyncResult(false, CurrentETag, PullRequests));
}

internal sealed class FakeGateReportRepository : IGateReportRepository
{
    public List<GateReport> Reports { get; } = [];

    public Task<GateReport?> FindLatestForWorkItemAsync(Guid workItemId, CancellationToken cancellationToken) =>
        Task.FromResult(Reports.LastOrDefault(r => r.WorkItemId == workItemId));

    public Task AddAsync(GateReport report, CancellationToken cancellationToken)
    {
        Reports.Add(report);
        return Task.CompletedTask;
    }
}

internal sealed class FakeGateLedgerRepository : IGateLedgerRepository
{
    public List<GateLedgerEntry> Entries { get; } = [];

    public Task<GateLedgerEntry?> FindTipAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Entries.OrderByDescending(e => e.Sequence).FirstOrDefault());

    public Task<IReadOnlyList<GateLedgerEntry>> ListForWorkItemAsync(Guid workItemId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<GateLedgerEntry>>(Entries);

    public Task AppendAsync(GateLedgerEntry entry, CancellationToken cancellationToken)
    {
        Entries.Add(entry);
        return Task.CompletedTask;
    }
}

internal sealed class FakeAgentEvidenceRepository : IAgentEvidenceRepository
{
    public List<AgentEvidence> Items { get; } = [];

    public Task<IReadOnlyList<AgentEvidence>> ListForWorkItemAsync(Guid workItemId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AgentEvidence>>(Items.Where(i => i.WorkItemId == workItemId).ToList());

    public Task AddAsync(AgentEvidence evidence, CancellationToken cancellationToken)
    {
        Items.Add(evidence);
        return Task.CompletedTask;
    }
}
