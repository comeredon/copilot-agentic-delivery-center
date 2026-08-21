using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Gates;

/// <summary>The stored result of one deterministic quality-gate run.</summary>
public sealed class GateReport : Entity
{
    private readonly List<GateCheckResult> _checks = [];

    private GateReport()
    {
        Commit = string.Empty;
        CanonicalHash = string.Empty;
        SchemaVersion = string.Empty;
    }

    public required string Commit { get; init; }

    public required QualityGateProfile Profile { get; init; }

    public string CanonicalHash { get; private set; }

    public string SchemaVersion { get; private set; }

    public GateVerdict Verdict { get; private set; } = GateVerdict.Pending;

    public Guid? WorkItemId { get; private set; }

    public IReadOnlyList<GateCheckResult> Checks => _checks;

    public static GateReport Create(
        string commit,
        QualityGateProfile profile,
        string schemaVersion,
        string canonicalHash,
        IEnumerable<GateCheckResult> checks,
        Guid? workItemId = null)
    {
        DomainException.ThrowIfNullOrWhiteSpace(commit, nameof(commit));
        DomainException.ThrowIfNullOrWhiteSpace(canonicalHash, nameof(canonicalHash));
        DomainException.ThrowIfNullOrWhiteSpace(schemaVersion, nameof(schemaVersion));

        var report = new GateReport
        {
            Commit = commit,
            Profile = profile,
            CanonicalHash = canonicalHash,
            SchemaVersion = schemaVersion,
            WorkItemId = workItemId,
        };

        // Canonical ordering is part of the determinism contract, so sort on ingest too.
        report._checks.AddRange(checks.OrderBy(c => c.Name, StringComparer.Ordinal));
        report.Verdict = report._checks.Count == 0
            ? GateVerdict.Inconclusive
            : report._checks.All(c => c.Passed) ? GateVerdict.Passed : GateVerdict.Failed;

        return report;
    }

    public bool SatisfiesRequirement(QualityGateProfile required) =>
        Verdict == GateVerdict.Passed && Profile >= required;
}

/// <summary>One named check inside a gate report.</summary>
public sealed record GateCheckResult(string Name, bool Passed, string Detail);
