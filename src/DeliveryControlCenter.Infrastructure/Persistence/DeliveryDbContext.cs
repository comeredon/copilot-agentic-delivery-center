using DeliveryControlCenter.Domain.Audit;
using DeliveryControlCenter.Domain.Delivery;
using DeliveryControlCenter.Domain.Evidence;
using DeliveryControlCenter.Domain.Gates;
using DeliveryControlCenter.Domain.Sync;
using Microsoft.EntityFrameworkCore;

namespace DeliveryControlCenter.Infrastructure.Persistence;

public sealed class DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : DbContext(options)
{
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();

    public DbSet<GateReport> GateReports => Set<GateReport>();

    public DbSet<GateLedgerEntry> GateLedgerEntries => Set<GateLedgerEntry>();

    public DbSet<AgentEvidence> Evidence => Set<AgentEvidence>();

    public DbSet<SyncCursor> SyncCursors => Set<SyncCursor>();

    public DbSet<RepositoryConnection> RepositoryConnections => Set<RepositoryConnection>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliveryDbContext).Assembly);
    }
}
