using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Domain.Audit;
using DeliveryControlCenter.Domain.Delivery;
using DeliveryControlCenter.Domain.Evidence;
using DeliveryControlCenter.Domain.Gates;
using DeliveryControlCenter.Domain.Sync;
using Microsoft.EntityFrameworkCore;

namespace DeliveryControlCenter.Infrastructure.Persistence;

internal sealed class WorkItemRepository(DeliveryDbContext db) : IWorkItemRepository
{
    public Task<WorkItem?> FindByNumberAsync(int number, CancellationToken cancellationToken) =>
        db.WorkItems.FirstOrDefaultAsync(w => w.Number == number, cancellationToken);

    public Task<WorkItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.WorkItems.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkItem>> ListAsync(CancellationToken cancellationToken) =>
        await db.WorkItems.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(WorkItem workItem, CancellationToken cancellationToken) =>
        await db.WorkItems.AddAsync(workItem, cancellationToken);
}

internal sealed class GateReportRepository(DeliveryDbContext db) : IGateReportRepository
{
    public Task<GateReport?> FindLatestForWorkItemAsync(Guid workItemId, CancellationToken cancellationToken) =>
        db.GateReports
            .AsNoTracking()
            .Include(r => r.Checks)
            .Where(r => r.WorkItemId == workItemId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(GateReport report, CancellationToken cancellationToken) =>
        await db.GateReports.AddAsync(report, cancellationToken);
}

internal sealed class GateLedgerRepository(DeliveryDbContext db) : IGateLedgerRepository
{
    public Task<GateLedgerEntry?> FindTipAsync(CancellationToken cancellationToken) =>
        db.GateLedgerEntries
            .AsNoTracking()
            .OrderByDescending(e => e.Sequence)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<GateLedgerEntry>> ListForWorkItemAsync(Guid workItemId, CancellationToken cancellationToken)
    {
        // The ledger is repository-wide; entries are correlated to a work item through the gate id.
        var prefix = workItemId.ToString("N");
        return await db.GateLedgerEntries
            .AsNoTracking()
            .Where(e => e.GateId.Contains(prefix))
            .OrderBy(e => e.Sequence)
            .ToListAsync(cancellationToken);
    }

    public async Task AppendAsync(GateLedgerEntry entry, CancellationToken cancellationToken) =>
        await db.GateLedgerEntries.AddAsync(entry, cancellationToken);
}

internal sealed class AgentEvidenceRepository(DeliveryDbContext db) : IAgentEvidenceRepository
{
    public async Task<IReadOnlyList<AgentEvidence>> ListForWorkItemAsync(Guid workItemId, CancellationToken cancellationToken) =>
        await db.Evidence
            .AsNoTracking()
            .Where(e => e.WorkItemId == workItemId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(AgentEvidence evidence, CancellationToken cancellationToken) =>
        await db.Evidence.AddAsync(evidence, cancellationToken);
}

internal sealed class SyncCursorRepository(DeliveryDbContext db) : ISyncCursorRepository
{
    public async Task<SyncCursor> GetOrCreateAsync(SyncFeed feed, CancellationToken cancellationToken)
    {
        var cursor = await db.SyncCursors.FirstOrDefaultAsync(c => c.Feed == feed, cancellationToken);
        if (cursor is not null)
        {
            return cursor;
        }

        cursor = SyncCursor.ForFeed(feed);
        await db.SyncCursors.AddAsync(cursor, cancellationToken);
        return cursor;
    }

    public async Task<IReadOnlyList<SyncCursor>> ListAsync(CancellationToken cancellationToken) =>
        await db.SyncCursors.AsNoTracking().ToListAsync(cancellationToken);
}

internal sealed class AuditLog(DeliveryDbContext db) : IAuditLog
{
    public async Task RecordAsync(AuditEvent auditEvent, CancellationToken cancellationToken) =>
        await db.AuditEvents.AddAsync(auditEvent, cancellationToken);

    public async Task<IReadOnlyList<AuditEvent>> ListRecentAsync(int take, CancellationToken cancellationToken) =>
        await db.AuditEvents
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
}

internal sealed class UnitOfWork(DeliveryDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
