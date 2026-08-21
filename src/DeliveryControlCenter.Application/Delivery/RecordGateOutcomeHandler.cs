using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Contracts.Realtime;
using DeliveryControlCenter.Domain.Audit;
using DeliveryControlCenter.Domain.Common;
using DeliveryControlCenter.Domain.Gates;
using Microsoft.Extensions.Logging;

namespace DeliveryControlCenter.Application.Delivery;

/// <summary>
/// Ingests one deterministic gate run and appends the cross-verification ledger entry.
/// The proposing and verifying agents must agree, otherwise the work item is parked
/// for a human rather than advanced.
/// </summary>
public sealed class RecordGateOutcomeHandler(
    IWorkItemRepository workItems,
    IGateReportRepository gateReports,
    IGateLedgerRepository ledger,
    IUnitOfWork unitOfWork,
    IDeliveryCache cache,
    IDeliveryNotifier notifier,
    IAuditLog auditLog,
    ILogger<RecordGateOutcomeHandler> logger)
{
    public async Task<GateLedgerEntry> HandleAsync(RecordGateOutcomeCommand command, CancellationToken cancellationToken)
    {
        var item = await workItems.FindByIdAsync(command.WorkItemId, cancellationToken)
            ?? throw new DomainException($"Work item {command.WorkItemId} was not found.");

        var report = GateReport.Create(
            command.Commit,
            command.Profile,
            command.SchemaVersion,
            command.CanonicalHash,
            command.Checks,
            item.Id);

        await gateReports.AddAsync(report, cancellationToken);

        var tip = await ledger.FindTipAsync(cancellationToken);
        var entry = GateLedgerEntry.Append(
            (tip?.Sequence ?? 0) + 1,
            command.GateId,
            command.Proposer,
            command.Verifier,
            command.CanonicalHash,
            tip?.EntryHash);

        await ledger.AppendAsync(entry, cancellationToken);

        if (entry.Agreed)
        {
            item.RecordGateResult(report.CanonicalHash, report.Verdict == GateVerdict.Passed);
        }
        else
        {
            logger.LogWarning(
                "Gate {GateId}: {Proposer} and {Verifier} disagreed; escalating to a human.",
                command.GateId,
                command.Proposer.Agent,
                command.Verifier.Agent);

            item.AdvanceTo(Domain.Delivery.AgentStage.NeedsHuman);
        }

        await auditLog.RecordAsync(
            AuditEvent.Allowed(
                command.Proposer.Agent,
                "gate.record",
                $"workitem:{item.Number}",
                $"{report.Verdict} agreed={entry.Agreed}"),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.InvalidateAsync(CacheKeys.Overview, cancellationToken);
        await cache.InvalidateAsync(CacheKeys.WorkItemList, cancellationToken);
        await cache.InvalidateAsync(CacheKeys.WorkItemDetail(item.Id), cancellationToken);
        await notifier.NotifyAsync(DeliveryHubContract.WorkItemChanged, item.Id, cancellationToken);

        return entry;
    }
}

public sealed record RecordGateOutcomeCommand(
    Guid WorkItemId,
    string GateId,
    string Commit,
    QualityGateProfile Profile,
    string SchemaVersion,
    string CanonicalHash,
    IReadOnlyList<GateCheckResult> Checks,
    AgentVerdict Proposer,
    AgentVerdict Verifier);
