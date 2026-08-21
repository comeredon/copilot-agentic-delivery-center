using System.Diagnostics;
using DeliveryControlCenter.Application.Configuration;
using DeliveryControlCenter.Application.Delivery;
using DeliveryControlCenter.Domain.Sync;
using Microsoft.Extensions.Options;

namespace DeliveryControlCenter.Worker;

/// <summary>
/// Polling loop over every feed. Polling was chosen over webhooks so the demo needs no public
/// ingress; ETags keep the rate-limit cost near zero when nothing changed.
/// </summary>
internal sealed class SyncWorker(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<DeliveryOptions> options,
    ILogger<SyncWorker> logger) : BackgroundService
{
    private static readonly SyncFeed[] Feeds = [SyncFeed.Issues, SyncFeed.PullRequests];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // A short initial delay lets migrations finish before the first read.
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCycleAsync(stoppingToken);

            var interval = options.CurrentValue.SyncInterval;
            using var timer = new PeriodicTimer(interval <= TimeSpan.Zero ? TimeSpan.FromMinutes(1) : interval);

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task RunCycleAsync(CancellationToken cancellationToken)
    {
        using var activity = DeliveryTelemetry.ActivitySource.StartActivity("sync.cycle", ActivityKind.Internal);

        foreach (var feed in Feeds)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<SynchronizeWorkItemsHandler>();
                var outcome = await handler.HandleAsync(feed, cancellationToken);

                logger.LogInformation(
                    "Sync cycle for {Feed} finished: succeeded={Succeeded} changed={Changed}.",
                    feed,
                    outcome.Succeeded,
                    outcome.ChangedCount);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                // One failing feed must not take the loop down.
                activity?.SetStatus(ActivityStatusCode.Error);
                logger.LogError(ex, "Unhandled failure while syncing {Feed}.", feed);
            }
        }
    }
}
