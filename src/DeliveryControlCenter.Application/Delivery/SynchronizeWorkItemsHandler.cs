using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Contracts.Realtime;
using DeliveryControlCenter.Domain.Audit;
using DeliveryControlCenter.Domain.Delivery;
using DeliveryControlCenter.Domain.Sync;
using Microsoft.Extensions.Logging;

namespace DeliveryControlCenter.Application.Delivery;

/// <summary>
/// Pulls one feed from GitHub and reconciles it into the local projection. Polling-first:
/// safe to run on a timer with no public endpoint, and idempotent by work-item number.
/// </summary>
public sealed class SynchronizeWorkItemsHandler(
    IGitHubReader gitHub,
    IWorkItemRepository workItems,
    ISyncCursorRepository cursors,
    IUnitOfWork unitOfWork,
    IDeliveryCache cache,
    IDeliveryNotifier notifier,
    IAuditLog auditLog,
    ILogger<SynchronizeWorkItemsHandler> logger)
{
    public async Task<SyncOutcome> HandleAsync(SyncFeed feed, CancellationToken cancellationToken)
    {
        var cursor = await cursors.GetOrCreateAsync(feed, cancellationToken);

        if (cursor.IsDeadLettered)
        {
            logger.LogWarning("Feed {Feed} is dead-lettered after {Failures} failures; skipping.", feed, cursor.ConsecutiveFailures);
            return SyncOutcome.Skipped;
        }

        try
        {
            var result = feed switch
            {
                SyncFeed.Issues => await gitHub.FetchIssuesAsync(cursor.ETag, cancellationToken),
                SyncFeed.PullRequests => await gitHub.FetchPullRequestsAsync(cursor.ETag, cancellationToken),
                _ => RemoteSyncResult.Unchanged(cursor.ETag),
            };

            if (result.NotModified)
            {
                cursor.RecordSuccess(DateTimeOffset.UtcNow, result.ETag);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return SyncOutcome.NotModified;
            }

            var changed = 0;
            foreach (var remote in result.Items)
            {
                var existing = await workItems.FindByNumberAsync(remote.Number, cancellationToken);
                if (existing is null)
                {
                    var created = WorkItem.Create(remote.Number, remote.Kind, remote.Title, remote.Url, remote.State);
                    await workItems.AddAsync(created, cancellationToken);
                    changed++;
                    continue;
                }

                if (existing.ApplyRemoteSnapshot(remote.Title, remote.Url, remote.State))
                {
                    changed++;
                }
            }

            cursor.RecordSuccess(DateTimeOffset.UtcNow, result.ETag);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            if (changed > 0)
            {
                await InvalidateReadModelAsync(cancellationToken);
                await notifier.NotifyAsync(DeliveryHubContract.OverviewChanged, null, cancellationToken);
            }

            await notifier.NotifyAsync(DeliveryHubContract.SyncHealthChanged, null, cancellationToken);
            return new SyncOutcome(true, changed);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Sync failed for feed {Feed}.", feed);
            cursor.RecordFailure(ex.GetType().Name);
            await auditLog.RecordAsync(
                AuditEvent.Blocked("system:worker", "sync", feed.ToString(), ex.GetType().Name),
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return SyncOutcome.Failed;
        }
    }

    private async Task InvalidateReadModelAsync(CancellationToken cancellationToken)
    {
        await cache.InvalidateAsync(CacheKeys.Overview, cancellationToken);
        await cache.InvalidateAsync(CacheKeys.WorkItemList, cancellationToken);
        await cache.InvalidateAsync(CacheKeys.SyncHealth, cancellationToken);
    }
}

public sealed record SyncOutcome(bool Succeeded, int ChangedCount)
{
    public static SyncOutcome Skipped { get; } = new(false, 0);

    public static SyncOutcome Failed { get; } = new(false, 0);

    public static SyncOutcome NotModified { get; } = new(true, 0);
}
