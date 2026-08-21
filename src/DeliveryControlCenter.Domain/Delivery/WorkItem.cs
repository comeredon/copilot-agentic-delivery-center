using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Delivery;

/// <summary>
/// A GitHub issue or pull request projected into the delivery model, plus the agentic
/// metadata the Project board tracks. GitHub remains the source of truth for
/// <see cref="Title"/>, <see cref="State"/> and <see cref="Url"/>.
/// </summary>
public sealed class WorkItem : Entity
{
    private WorkItem()
    {
        Title = string.Empty;
        Url = string.Empty;
        Workstream = string.Empty;
    }

    public required int Number { get; init; }

    public required WorkItemKind Kind { get; init; }

    public string Title { get; private set; }

    public string Url { get; private set; }

    public WorkItemState State { get; private set; }

    public AgentStage Stage { get; private set; } = AgentStage.Unassigned;

    public RiskLevel Risk { get; private set; } = RiskLevel.Low;

    public string Workstream { get; private set; }

    /// <summary>Hash of the newest canonical gate report. Null until a gate has run.</summary>
    public string? LatestGateHash { get; private set; }

    /// <summary>Set only by an owner-issued authorization signal. Agents can never set this.</summary>
    public bool HumanAuthorized { get; private set; }

    public static WorkItem Create(int number, WorkItemKind kind, string title, string url, WorkItemState state)
    {
        DomainException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        DomainException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        if (number <= 0)
        {
            throw new DomainException("Work item number must be positive.");
        }

        return new WorkItem
        {
            Number = number,
            Kind = kind,
            Title = title,
            Url = url,
            State = state,
        };
    }

    /// <summary>
    /// Applies a fresh GitHub projection. Agentic metadata is untouched. Returns false when
    /// nothing actually changed, so a steady-state poll does not churn the cache.
    /// </summary>
    public bool ApplyRemoteSnapshot(string title, string url, WorkItemState state)
    {
        DomainException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        DomainException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        if (string.Equals(Title, title, StringComparison.Ordinal)
            && string.Equals(Url, url, StringComparison.Ordinal)
            && State == state)
        {
            return false;
        }

        Title = title;
        Url = url;
        State = state;
        Touch();
        return true;
    }

    public void Classify(RiskLevel risk, string workstream)
    {
        Risk = risk;
        Workstream = workstream ?? string.Empty;
        Touch();
    }

    public void AdvanceTo(AgentStage stage)
    {
        if (Stage == AgentStage.Merged && stage != AgentStage.Merged)
        {
            throw new DomainException("A merged work item cannot re-enter the delivery flow.");
        }

        Stage = stage;
        Touch();
    }

    public void RecordGateResult(string canonicalHash, bool passed)
    {
        DomainException.ThrowIfNullOrWhiteSpace(canonicalHash, nameof(canonicalHash));

        LatestGateHash = canonicalHash;
        Stage = passed ? AgentStage.GatePassed : AgentStage.GateFailed;
        Touch();
    }

    /// <summary>
    /// The human authorization signal. Requires a passing gate first, so a green
    /// report can never be skipped on the way to merge.
    /// </summary>
    public void AuthorizeByHuman()
    {
        if (Stage != AgentStage.GatePassed && Stage != AgentStage.AwaitingHumanAuthorization)
        {
            throw new DomainException("Human authorization requires a passing quality gate.");
        }

        HumanAuthorized = true;
        Stage = AgentStage.AwaitingHumanAuthorization;
        Touch();
    }
}
