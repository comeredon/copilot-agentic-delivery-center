namespace DeliveryControlCenter.Contracts.Delivery;

/// <summary>Top-level dashboard projection. Cached; refreshed after every sync cycle.</summary>
public sealed record DeliveryOverviewDto(
    string Repository,
    int OpenIssues,
    int OpenPullRequests,
    int AwaitingHumanAuthorization,
    int GateFailures,
    int NeedsHuman,
    double GatePassRate,
    DateTimeOffset? LastSyncedAtUtc,
    IReadOnlyList<StageCountDto> StageBreakdown);

public sealed record StageCountDto(string Stage, int Count);

public sealed record WorkItemSummaryDto(
    Guid Id,
    int Number,
    string Kind,
    string Title,
    string State,
    string Stage,
    string Risk,
    string Workstream,
    string Url,
    bool HumanAuthorized,
    DateTimeOffset UpdatedAtUtc);

public sealed record WorkItemDetailDto(
    WorkItemSummaryDto Summary,
    GateReportDto? LatestGate,
    IReadOnlyList<AgentEvidenceDto> Evidence,
    IReadOnlyList<GateLedgerEntryDto> Ledger);

public sealed record GateReportDto(
    string Commit,
    string Profile,
    string Verdict,
    string CanonicalHash,
    string SchemaVersion,
    IReadOnlyList<GateCheckDto> Checks);

public sealed record GateCheckDto(string Name, bool Passed, string Detail);

public sealed record GateLedgerEntryDto(
    long Sequence,
    string GateId,
    string ProposerAgent,
    string ProposerModel,
    string ProposerVerdict,
    string VerifierAgent,
    string VerifierModel,
    string VerifierVerdict,
    bool Agreed,
    string EntryHash,
    string PreviousEntryHash);

public sealed record AgentEvidenceDto(
    string Source,
    string Severity,
    string Summary,
    string Model,
    DateTimeOffset RecordedAtUtc);
