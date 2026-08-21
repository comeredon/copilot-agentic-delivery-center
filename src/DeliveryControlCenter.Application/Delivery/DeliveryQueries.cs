using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Application.Configuration;
using DeliveryControlCenter.Contracts.Delivery;
using DeliveryControlCenter.Contracts.Operations;
using DeliveryControlCenter.Domain.Delivery;
using Microsoft.Extensions.Options;

namespace DeliveryControlCenter.Application.Delivery;

/// <summary>
/// Read side of the control center. Every query is cache-aside: the cache is consulted first,
/// the database is the fallback, and writes invalidate explicitly.
/// </summary>
public sealed class DeliveryQueries(
    IWorkItemRepository workItems,
    IGateReportRepository gateReports,
    IGateLedgerRepository ledger,
    IAgentEvidenceRepository evidence,
    ISyncCursorRepository cursors,
    IDeliveryCache cache,
    IOptions<DeliveryOptions> options)
{
    private static readonly TimeSpan ShortLocal = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan ShortDistributed = TimeSpan.FromMinutes(1);

    private readonly DeliveryOptions _options = options.Value;

    public Task<DeliveryOverviewDto> GetOverviewAsync(CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(
            CacheKeys.Overview,
            async ct => await BuildOverviewAsync(ct),
            ShortLocal,
            ShortDistributed,
            cancellationToken);

    public Task<IReadOnlyList<WorkItemSummaryDto>> GetWorkItemsAsync(CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync<IReadOnlyList<WorkItemSummaryDto>>(
            CacheKeys.WorkItemList,
            async ct =>
            {
                var items = await workItems.ListAsync(ct);
                return [.. items.OrderByDescending(i => i.UpdatedAtUtc).Select(i => i.ToSummary())];
            },
            ShortLocal,
            ShortDistributed,
            cancellationToken);

    public async Task<WorkItemDetailDto?> GetWorkItemDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await workItems.FindByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return null;
        }

        var latestGate = await gateReports.FindLatestForWorkItemAsync(id, cancellationToken);
        var findings = await evidence.ListForWorkItemAsync(id, cancellationToken);
        var ledgerEntries = await ledger.ListForWorkItemAsync(id, cancellationToken);

        return new WorkItemDetailDto(
            item.ToSummary(),
            latestGate?.ToDto(),
            [.. findings.OrderByDescending(f => f.Severity).Select(f => f.ToDto())],
            [.. ledgerEntries.OrderBy(e => e.Sequence).Select(e => e.ToDto())]);
    }

    public async Task<SyncHealthDto> GetSyncHealthAsync(CancellationToken cancellationToken)
    {
        var all = await cursors.ListAsync(cancellationToken);
        return new SyncHealthDto(
            [.. all.OrderBy(c => c.Feed).Select(c => c.ToDto())],
            _options.EnableProjectWrites);
    }

    public CacheHealthDto GetCacheHealth()
    {
        var counters = cache.Snapshot();
        return new CacheHealthDto(
            counters.Hits,
            counters.Misses,
            counters.Invalidations,
            counters.Errors,
            counters.HitRatio);
    }

    private async ValueTask<DeliveryOverviewDto> BuildOverviewAsync(CancellationToken cancellationToken)
    {
        var items = await workItems.ListAsync(cancellationToken);
        var allCursors = await cursors.ListAsync(cancellationToken);

        var gated = items.Where(i => i.Stage is AgentStage.GatePassed or AgentStage.GateFailed).ToList();
        var passRate = gated.Count == 0
            ? 0d
            : (double)gated.Count(i => i.Stage == AgentStage.GatePassed) / gated.Count;

        var stageBreakdown = items
            .GroupBy(i => i.Stage)
            .OrderBy(g => g.Key)
            .Select(g => new StageCountDto(g.Key.ToString(), g.Count()))
            .ToList();

        return new DeliveryOverviewDto(
            _options.RepositoryFullName,
            items.Count(i => i.Kind == WorkItemKind.Issue && i.State == WorkItemState.Open),
            items.Count(i => i.Kind == WorkItemKind.PullRequest && i.State is WorkItemState.Open or WorkItemState.Draft),
            items.Count(i => i.Stage == AgentStage.AwaitingHumanAuthorization),
            items.Count(i => i.Stage == AgentStage.GateFailed),
            items.Count(i => i.Stage == AgentStage.NeedsHuman),
            passRate,
            allCursors.Where(c => c.LastSyncedAtUtc is not null).Max(c => c.LastSyncedAtUtc),
            stageBreakdown);
    }
}
