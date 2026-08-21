using DeliveryControlCenter.Domain.Audit;
using DeliveryControlCenter.Domain.Delivery;
using DeliveryControlCenter.Domain.Evidence;
using DeliveryControlCenter.Domain.Gates;
using DeliveryControlCenter.Domain.Sync;

namespace DeliveryControlCenter.Application.Abstractions;

public interface IWorkItemRepository
{
    Task<WorkItem?> FindByNumberAsync(int number, CancellationToken cancellationToken);

    Task<WorkItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<WorkItem>> ListAsync(CancellationToken cancellationToken);

    Task AddAsync(WorkItem workItem, CancellationToken cancellationToken);
}

public interface IGateReportRepository
{
    Task<GateReport?> FindLatestForWorkItemAsync(Guid workItemId, CancellationToken cancellationToken);

    Task AddAsync(GateReport report, CancellationToken cancellationToken);
}

public interface IGateLedgerRepository
{
    Task<GateLedgerEntry?> FindTipAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<GateLedgerEntry>> ListForWorkItemAsync(Guid workItemId, CancellationToken cancellationToken);

    Task AppendAsync(GateLedgerEntry entry, CancellationToken cancellationToken);
}

public interface IAgentEvidenceRepository
{
    Task<IReadOnlyList<AgentEvidence>> ListForWorkItemAsync(Guid workItemId, CancellationToken cancellationToken);

    Task AddAsync(AgentEvidence evidence, CancellationToken cancellationToken);
}

public interface ISyncCursorRepository
{
    Task<SyncCursor> GetOrCreateAsync(SyncFeed feed, CancellationToken cancellationToken);

    Task<IReadOnlyList<SyncCursor>> ListAsync(CancellationToken cancellationToken);
}

public interface IAuditLog
{
    Task RecordAsync(AuditEvent auditEvent, CancellationToken cancellationToken);

    Task<IReadOnlyList<AuditEvent>> ListRecentAsync(int take, CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
