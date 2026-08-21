using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Application.Delivery;
using DeliveryControlCenter.Domain.Delivery;
using DeliveryControlCenter.Domain.Sync;
using DeliveryControlCenter.UnitTests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeliveryControlCenter.UnitTests.Application;

public sealed class SynchronizeWorkItemsHandlerTests
{
    [Fact]
    public async Task NotModified_DoesNotTouchTheReadModel()
    {
        var context = new TestContext();
        context.GitHub.CurrentETag = "\"v1\"";
        context.Cursors.Cursors.Add(Seeded("\"v1\""));

        var outcome = await context.Handler.HandleAsync(SyncFeed.Issues, CancellationToken.None);

        outcome.ShouldBe(SyncOutcome.NotModified);
        context.Cache.Invalidated.ShouldBeEmpty();
        context.WorkItems.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task FirstSync_CreatesItemsAndInvalidatesTheReadModel()
    {
        var context = new TestContext();
        context.GitHub.Issues.Add(new RemoteWorkItem(1, WorkItemKind.Issue, "First", "https://example.test/1", WorkItemState.Open));

        var outcome = await context.Handler.HandleAsync(SyncFeed.Issues, CancellationToken.None);

        outcome.Succeeded.ShouldBeTrue();
        outcome.ChangedCount.ShouldBe(1);
        context.WorkItems.Items.Count.ShouldBe(1);
        context.Cache.Invalidated.ShouldContain(CacheKeys.Overview);
        context.Cache.Invalidated.ShouldContain(CacheKeys.WorkItemList);
    }

    [Fact]
    public async Task RepeatedSync_WithIdenticalPayload_ChangesNothing()
    {
        var context = new TestContext();
        context.GitHub.Issues.Add(new RemoteWorkItem(1, WorkItemKind.Issue, "First", "https://example.test/1", WorkItemState.Open));

        await context.Handler.HandleAsync(SyncFeed.Issues, CancellationToken.None);
        context.Cache.Invalidated.Clear();

        // Force a payload transfer by changing the ETag, but keep the content identical.
        context.GitHub.CurrentETag = "\"v2\"";
        var second = await context.Handler.HandleAsync(SyncFeed.Issues, CancellationToken.None);

        second.ChangedCount.ShouldBe(0);
        context.Cache.Invalidated.ShouldBeEmpty();
        context.WorkItems.Items.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Failure_RecordsAnAuditEventAndAdvancesTheFailureStreak()
    {
        var context = new TestContext();
        context.GitHub.ThrowOnFetch = true;

        var outcome = await context.Handler.HandleAsync(SyncFeed.Issues, CancellationToken.None);

        outcome.Succeeded.ShouldBeFalse();
        context.Audit.Events.ShouldContain(e => e.Action == "sync" && e.Denied);
        context.Cursors.Cursors.Single().ConsecutiveFailures.ShouldBe(1);
    }

    [Fact]
    public async Task DeadLetteredFeed_IsSkippedWithoutCallingGitHub()
    {
        var context = new TestContext();
        var cursor = SyncCursor.ForFeed(SyncFeed.Issues);
        for (var i = 0; i < 5; i++)
        {
            cursor.RecordFailure("boom");
        }

        context.Cursors.Cursors.Add(cursor);

        var outcome = await context.Handler.HandleAsync(SyncFeed.Issues, CancellationToken.None);

        outcome.ShouldBe(SyncOutcome.Skipped);
        context.GitHub.IssueCallCount.ShouldBe(0);
    }

    private static SyncCursor Seeded(string etag)
    {
        var cursor = SyncCursor.ForFeed(SyncFeed.Issues);
        cursor.RecordSuccess(DateTimeOffset.UtcNow, etag);
        return cursor;
    }

    private sealed class TestContext
    {
        public FakeGitHubReader GitHub { get; } = new();

        public FakeWorkItemRepository WorkItems { get; } = new();

        public FakeSyncCursorRepository Cursors { get; } = new();

        public FakeUnitOfWork UnitOfWork { get; } = new();

        public FakeCache Cache { get; } = new();

        public FakeNotifier Notifier { get; } = new();

        public FakeAuditLog Audit { get; } = new();

        public SynchronizeWorkItemsHandler Handler => new(
            GitHub,
            WorkItems,
            Cursors,
            UnitOfWork,
            Cache,
            Notifier,
            Audit,
            NullLogger<SynchronizeWorkItemsHandler>.Instance);
    }
}
