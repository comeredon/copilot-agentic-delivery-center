using DeliveryControlCenter.Contracts.Delivery;
using DeliveryControlCenter.Contracts.Operations;
using DeliveryControlCenter.Domain.Audit;
using DeliveryControlCenter.Domain.Delivery;
using DeliveryControlCenter.Domain.Evidence;
using DeliveryControlCenter.Domain.Gates;
using DeliveryControlCenter.Domain.Sync;

namespace DeliveryControlCenter.Application.Delivery;

/// <summary>Domain to contract projection. Kept in one place so the API and UI never see entities.</summary>
public static class DeliveryMapping
{
    public static WorkItemSummaryDto ToSummary(this WorkItem item) => new(
        item.Id,
        item.Number,
        item.Kind.ToString(),
        item.Title,
        item.State.ToString(),
        item.Stage.ToString(),
        item.Risk.ToString(),
        item.Workstream,
        item.Url,
        item.HumanAuthorized,
        item.UpdatedAtUtc);

    public static GateReportDto ToDto(this GateReport report) => new(
        report.Commit,
        report.Profile.ToString(),
        report.Verdict.ToString(),
        report.CanonicalHash,
        report.SchemaVersion,
        [.. report.Checks.Select(c => new GateCheckDto(c.Name, c.Passed, c.Detail))]);

    public static GateLedgerEntryDto ToDto(this GateLedgerEntry entry) => new(
        entry.Sequence,
        entry.GateId,
        entry.Proposer.Agent,
        entry.Proposer.Model,
        entry.Proposer.Verdict.ToString(),
        entry.Verifier.Agent,
        entry.Verifier.Model,
        entry.Verifier.Verdict.ToString(),
        entry.Agreed,
        entry.EntryHash,
        entry.PreviousEntryHash);

    public static AgentEvidenceDto ToDto(this AgentEvidence evidence) => new(
        evidence.Source.ToString(),
        evidence.Severity.ToString(),
        evidence.Summary,
        evidence.Model,
        evidence.CreatedAtUtc);

    public static SyncFeedHealthDto ToDto(this SyncCursor cursor) => new(
        cursor.Feed.ToString(),
        cursor.LastSyncedAtUtc,
        cursor.ConsecutiveFailures,
        cursor.IsDeadLettered,
        cursor.LastError);

    public static AuditEventDto ToDto(this AuditEvent auditEvent) => new(
        auditEvent.CreatedAtUtc,
        auditEvent.Actor,
        auditEvent.Action,
        auditEvent.Target,
        auditEvent.Outcome,
        auditEvent.Denied);
}
