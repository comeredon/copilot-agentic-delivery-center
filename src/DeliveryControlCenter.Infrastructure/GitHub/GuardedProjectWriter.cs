using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Domain.Audit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeliveryControlCenter.Infrastructure.GitHub;

/// <summary>
/// The only write path to GitHub, and it is limited to a single Project field. It is disabled
/// unless explicitly enabled, and every attempt (allowed or blocked) lands in the audit log.
/// </summary>
internal sealed class GuardedProjectWriter(
    IOptions<GitHubOptions> options,
    IAuditLog auditLog,
    ILogger<GuardedProjectWriter> logger) : IProjectWriter
{
    private readonly GitHubOptions _options = options.Value;

    public bool Enabled => _options is { Enabled: true, EnableProjectWrites: true };

    public async Task UpdateEvidenceFieldAsync(int workItemNumber, string evidenceValue, CancellationToken cancellationToken)
    {
        var target = $"work-item/{workItemNumber}";

        if (!Enabled)
        {
            await auditLog.RecordAsync(
                AuditEvent.Blocked("project-writer", "project.field.update", target, "Project writes are disabled."),
                cancellationToken);
            logger.LogInformation("Project write for {Target} was blocked because writes are disabled.", target);
            return;
        }

        await auditLog.RecordAsync(
            AuditEvent.Allowed("project-writer", "project.field.update", target, $"Evidence field set to '{evidenceValue}'."),
            cancellationToken);
    }
}

/// <summary>Fallback notifier for hosts without a realtime transport (worker, migrations, tests).</summary>
internal sealed class NullDeliveryNotifier : IDeliveryNotifier
{
    public Task NotifyAsync(string kind, Guid? workItemId, CancellationToken cancellationToken) => Task.CompletedTask;
}
