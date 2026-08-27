using DeliveryControlCenter.Application.Catalog;

namespace DeliveryControlCenter.Worker;

/// <summary>
/// Keeps the public catalog warm. Freshness transitions and low-stock notifications are reserved
/// as visible follow-up slices for the Copilot demonstration backlog.
/// </summary>
internal sealed class CoffeeOperationsWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<CoffeeOperationsWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var queries = scope.ServiceProvider.GetRequiredService<CatalogQueries>();
                var catalog = await queries.SearchAsync(new CatalogFilter(PageSize: 48), stoppingToken);
                logger.LogInformation("Ninth Parallel catalog warm: {CoffeeCount} active coffees.", catalog.Total);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Catalog warming failed; requests will fall back to direct reads.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
