using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Application.Delivery;
using DeliveryControlCenter.Domain.Common;
using DeliveryControlCenter.Domain.Delivery;
using DeliveryControlCenter.UnitTests.Fakes;

namespace DeliveryControlCenter.UnitTests.Application;

public sealed class AuthorizeWorkItemHandlerTests
{
    [Fact]
    public async Task NonOwner_IsRefusedAndAudited()
    {
        var context = new TestContext();
        var item = GatedItem();
        context.WorkItems.Items.Add(item);

        var authorized = await context.Handler.HandleAsync(
            new AuthorizeWorkItemCommand(item.Id, "delivery-orchestrator", ActorIsRepositoryOwner: false),
            CancellationToken.None);

        authorized.ShouldBeFalse();
        item.HumanAuthorized.ShouldBeFalse();
        context.Audit.Events.ShouldContain(e => e.Denied && e.Action == "workitem.authorize");
    }

    [Fact]
    public async Task Owner_AuthorizesAndInvalidatesTheReadModel()
    {
        var context = new TestContext();
        var item = GatedItem();
        context.WorkItems.Items.Add(item);

        var authorized = await context.Handler.HandleAsync(
            new AuthorizeWorkItemCommand(item.Id, "comeredon", ActorIsRepositoryOwner: true),
            CancellationToken.None);

        authorized.ShouldBeTrue();
        item.HumanAuthorized.ShouldBeTrue();
        context.Cache.Invalidated.ShouldContain(CacheKeys.Overview);
        context.Notifier.Notifications.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Owner_CannotAuthorizeBeforeTheGate()
    {
        var context = new TestContext();
        var item = WorkItem.Create(1, WorkItemKind.PullRequest, "PR", "https://example.test/1", WorkItemState.Open);
        context.WorkItems.Items.Add(item);

        await Should.ThrowAsync<DomainException>(() => context.Handler.HandleAsync(
            new AuthorizeWorkItemCommand(item.Id, "comeredon", ActorIsRepositoryOwner: true),
            CancellationToken.None));
    }

    [Fact]
    public async Task UnknownWorkItem_Throws()
    {
        var context = new TestContext();

        await Should.ThrowAsync<DomainException>(() => context.Handler.HandleAsync(
            new AuthorizeWorkItemCommand(Guid.CreateVersion7(), "comeredon", ActorIsRepositoryOwner: true),
            CancellationToken.None));
    }

    private static WorkItem GatedItem()
    {
        var item = WorkItem.Create(1, WorkItemKind.PullRequest, "PR", "https://example.test/1", WorkItemState.Open);
        item.RecordGateResult("sha256:" + new string('a', 64), passed: true);
        return item;
    }

    private sealed class TestContext
    {
        public FakeWorkItemRepository WorkItems { get; } = new();

        public FakeUnitOfWork UnitOfWork { get; } = new();

        public FakeCache Cache { get; } = new();

        public FakeNotifier Notifier { get; } = new();

        public FakeAuditLog Audit { get; } = new();

        public AuthorizeWorkItemHandler Handler => new(WorkItems, UnitOfWork, Cache, Notifier, Audit);
    }
}
