using DeliveryControlCenter.Domain.Audit;
using DeliveryControlCenter.Domain.Common;
using DeliveryControlCenter.Domain.Delivery;
using DeliveryControlCenter.Domain.Evidence;
using DeliveryControlCenter.Domain.Gates;
using DeliveryControlCenter.Domain.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryControlCenter.Infrastructure.Persistence.Configurations;

internal static class EntityBuilderExtensions
{
    /// <summary>
    /// Shared identity and optimistic-concurrency mapping. <c>Version</c> is a domain-owned token
    /// rather than a provider system column, so the same mapping works on any relational provider.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ApplyEntityConventions<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : Entity
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Version).IsConcurrencyToken().HasConversion<long>();
        return builder;
    }
}

public sealed class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("work_items").ApplyEntityConventions();

        builder.HasIndex(e => e.Number).IsUnique();
        builder.Property(e => e.Title).HasMaxLength(512).IsRequired();
        builder.Property(e => e.Url).HasMaxLength(512).IsRequired();
        builder.Property(e => e.Workstream).HasMaxLength(128);
        builder.Property(e => e.LatestGateHash).HasMaxLength(80);
        builder.Property(e => e.Kind).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.State).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.Stage).HasConversion<string>().HasMaxLength(48);
        builder.Property(e => e.Risk).HasConversion<string>().HasMaxLength(16);
    }
}

public sealed class GateReportConfiguration : IEntityTypeConfiguration<GateReport>
{
    public void Configure(EntityTypeBuilder<GateReport> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("gate_reports").ApplyEntityConventions();

        builder.Property(e => e.Commit).HasMaxLength(64).IsRequired();
        builder.Property(e => e.CanonicalHash).HasMaxLength(80).IsRequired();
        builder.Property(e => e.SchemaVersion).HasMaxLength(32).IsRequired();
        builder.Property(e => e.Profile).HasConversion<string>().HasMaxLength(16);
        builder.Property(e => e.Verdict).HasConversion<string>().HasMaxLength(16);
        builder.HasIndex(e => new { e.WorkItemId, e.CreatedAtUtc });

        builder.OwnsMany(e => e.Checks, checks =>
        {
            checks.ToTable("gate_check_results");
            checks.WithOwner().HasForeignKey("GateReportId");
            checks.Property<int>("Ordinal");
            checks.HasKey("GateReportId", "Ordinal");
            checks.Property(c => c.Name).HasMaxLength(128).IsRequired();
            checks.Property(c => c.Detail).HasMaxLength(2048);
        });

        builder.Navigation(e => e.Checks).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class GateLedgerEntryConfiguration : IEntityTypeConfiguration<GateLedgerEntry>
{
    public void Configure(EntityTypeBuilder<GateLedgerEntry> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("gate_ledger_entries").ApplyEntityConventions();

        builder.HasIndex(e => e.Sequence).IsUnique();
        builder.Property(e => e.GateId).HasMaxLength(128).IsRequired();
        builder.Property(e => e.EvidenceHash).HasMaxLength(80).IsRequired();
        builder.Property(e => e.PreviousEntryHash).HasMaxLength(80).IsRequired();
        builder.Property(e => e.EntryHash).HasMaxLength(80).IsRequired();

        builder.ComplexProperty(e => e.Proposer, p =>
        {
            p.Property(v => v.Agent).HasColumnName("proposer_agent").HasMaxLength(64);
            p.Property(v => v.Model).HasColumnName("proposer_model").HasMaxLength(64);
            p.Property(v => v.Verdict).HasColumnName("proposer_verdict").HasConversion<string>().HasMaxLength(16);
        });

        builder.ComplexProperty(e => e.Verifier, p =>
        {
            p.Property(v => v.Agent).HasColumnName("verifier_agent").HasMaxLength(64);
            p.Property(v => v.Model).HasColumnName("verifier_model").HasMaxLength(64);
            p.Property(v => v.Verdict).HasColumnName("verifier_verdict").HasConversion<string>().HasMaxLength(16);
        });
    }
}

public sealed class AgentEvidenceConfiguration : IEntityTypeConfiguration<AgentEvidence>
{
    public void Configure(EntityTypeBuilder<AgentEvidence> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("agent_evidence").ApplyEntityConventions();

        builder.HasIndex(e => e.WorkItemId);
        builder.Property(e => e.Summary).HasMaxLength(4096).IsRequired();
        builder.Property(e => e.Model).HasMaxLength(64);
        builder.Property(e => e.Source).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.Severity).HasConversion<string>().HasMaxLength(16);
    }
}

public sealed class SyncCursorConfiguration : IEntityTypeConfiguration<SyncCursor>
{
    public void Configure(EntityTypeBuilder<SyncCursor> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("sync_cursors").ApplyEntityConventions();

        builder.Ignore(e => e.IsDeadLettered);
        builder.HasIndex(e => e.Feed).IsUnique();
        builder.Property(e => e.Feed).HasConversion<string>().HasMaxLength(32);
        builder.Property(e => e.ETag).HasMaxLength(256);
        builder.Property(e => e.LastError).HasMaxLength(512);
    }
}

public sealed class RepositoryConnectionConfiguration : IEntityTypeConfiguration<RepositoryConnection>
{
    public void Configure(EntityTypeBuilder<RepositoryConnection> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("repository_connections").ApplyEntityConventions();

        builder.Ignore(e => e.FullName);
        builder.Property(e => e.Owner).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(128).IsRequired();
        builder.HasIndex(e => new { e.Owner, e.Name }).IsUnique();
    }
}

public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("audit_events").ApplyEntityConventions();

        builder.HasIndex(e => e.CreatedAtUtc);
        builder.Property(e => e.Actor).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Action).HasMaxLength(128).IsRequired();
        builder.Property(e => e.Target).HasMaxLength(256);
        builder.Property(e => e.Outcome).HasMaxLength(512);
    }
}
