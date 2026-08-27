using System.Diagnostics;
using DeliveryControlCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeliveryControlCenter.MigrationService;

/// <summary>
/// Applies migrations once, then stops the host. The API and worker depend on this completing,
/// which keeps schema changes out of request-serving startup paths.
/// </summary>
internal sealed class MigrationWorker(
    IServiceScopeFactory scopeFactory,
    IHostApplicationLifetime lifetime,
    ILogger<MigrationWorker> logger) : BackgroundService
{
    public const string ActivitySourceName = "DeliveryControlCenter.Migrations";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = DeliveryTelemetry.ActivitySource.StartActivity("database.migrate", ActivityKind.Client);

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();

            var strategy = db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () => await db.Database.MigrateAsync(stoppingToken));
            await StoreDataSeeder.SeedAsync(db, stoppingToken);

            logger.LogInformation("Database migration and deterministic store seeding completed.");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            logger.LogError(ex, "Database migration failed.");
            throw;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }
}
