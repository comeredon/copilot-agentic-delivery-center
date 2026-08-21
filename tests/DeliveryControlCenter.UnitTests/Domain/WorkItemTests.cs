using DeliveryControlCenter.Domain.Common;
using DeliveryControlCenter.Domain.Delivery;

namespace DeliveryControlCenter.UnitTests.Domain;

public sealed class WorkItemTests
{
    [Fact]
    public void Create_StartsUnassigned()
    {
        var item = NewItem();

        item.Stage.ShouldBe(AgentStage.Unassigned);
        item.HumanAuthorized.ShouldBeFalse();
    }

    [Fact]
    public void AuthorizeByHuman_IsRejectedBeforeTheGate()
    {
        var item = NewItem();

        // This is the whole point of the guardrail: no gate, no authorization.
        Should.Throw<DomainException>(item.AuthorizeByHuman);
    }

    [Fact]
    public void AuthorizeByHuman_IsAllowedOnceTheGatePassed()
    {
        var item = NewItem();
        item.AdvanceTo(AgentStage.GatePassed);

        item.AuthorizeByHuman();

        item.HumanAuthorized.ShouldBeTrue();
        item.Stage.ShouldBe(AgentStage.AwaitingHumanAuthorization);
    }

    [Fact]
    public void ApplyRemoteSnapshot_IsIdempotentForUnchangedInput()
    {
        var item = NewItem();
        item.ApplyRemoteSnapshot("Title", "https://example.test/1", WorkItemState.Open);
        var firstVersion = item.Version;

        item.ApplyRemoteSnapshot("Title", "https://example.test/1", WorkItemState.Open);

        item.Version.ShouldBe(firstVersion);
    }

    [Fact]
    public void ApplyRemoteSnapshot_BumpsVersionWhenSomethingChanged()
    {
        var item = NewItem();
        var firstVersion = item.Version;

        item.ApplyRemoteSnapshot("New title", "https://example.test/1", WorkItemState.Closed);

        item.Version.ShouldBeGreaterThan(firstVersion);
        item.State.ShouldBe(WorkItemState.Closed);
    }

    private static WorkItem NewItem() =>
        WorkItem.Create(1, WorkItemKind.Issue, "Title", "https://example.test/1", WorkItemState.Open);
}
