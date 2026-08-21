using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Evidence;

/// <summary>Which advisory reviewer produced a finding.</summary>
public enum EvidenceSource
{
    RubberDuck = 0,
    SecurityReview = 1,
    CodeReview = 2,
    ThreatModel = 3,
    TestReview = 4,
}

/// <summary>Mirrors the severity vocabulary the built-in reviewers report.</summary>
public enum EvidenceSeverity
{
    Suggestion = 0,
    NonBlocking = 1,
    Blocking = 2,
}

/// <summary>
/// A normalized advisory finding. Advisory evidence is recorded and surfaced, but it can
/// never turn a failing deterministic check green.
/// </summary>
public sealed class AgentEvidence : Entity
{
    private AgentEvidence()
    {
        Summary = string.Empty;
        Model = string.Empty;
    }

    public required Guid WorkItemId { get; init; }

    public required EvidenceSource Source { get; init; }

    public required EvidenceSeverity Severity { get; init; }

    public string Summary { get; private set; }

    public string Model { get; private set; }

    public static AgentEvidence Record(
        Guid workItemId,
        EvidenceSource source,
        EvidenceSeverity severity,
        string summary,
        string model)
    {
        DomainException.ThrowIfNullOrWhiteSpace(summary, nameof(summary));

        return new AgentEvidence
        {
            WorkItemId = workItemId,
            Source = source,
            Severity = severity,
            Summary = summary,
            Model = model ?? string.Empty,
        };
    }
}
