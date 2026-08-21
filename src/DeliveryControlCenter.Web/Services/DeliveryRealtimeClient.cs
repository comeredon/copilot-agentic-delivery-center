using DeliveryControlCenter.Contracts.Realtime;
using Microsoft.AspNetCore.SignalR.Client;

namespace DeliveryControlCenter.Web.Services;

/// <summary>
/// Shared hub connection. Pushes are treated as hints only: components refetch through the
/// API so the UI never renders state that did not come through the audited read path.
/// </summary>
public sealed class DeliveryRealtimeClient(IConfiguration configuration, ILogger<DeliveryRealtimeClient> logger)
    : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private HubConnection? _connection;

    public event Func<DeliveryNotification, Task>? Changed;

    public async Task<bool> EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { State: HubConnectionState.Connected })
        {
            return true;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_connection is null)
            {
                var apiBase = configuration["services:api:https:0"]
                              ?? configuration["services:api:http:0"]
                              ?? "http://localhost:5100";

                _connection = new HubConnectionBuilder()
                    .WithUrl(new Uri(new Uri(apiBase), DeliveryHubContract.Path))
                    .WithAutomaticReconnect()
                    .Build();

                foreach (var kind in new[]
                         {
                             DeliveryHubContract.OverviewChanged,
                             DeliveryHubContract.WorkItemChanged,
                             DeliveryHubContract.SyncHealthChanged,
                         })
                {
                    _connection.On<DeliveryNotification>(kind, async notification =>
                    {
                        if (Changed is not null)
                        {
                            await Changed.Invoke(notification);
                        }
                    });
                }
            }

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync(cancellationToken);
            }

            return _connection.State == HubConnectionState.Connected;
        }
        catch (Exception ex)
        {
            // Realtime is an enhancement; the pages still work with manual refresh.
            logger.LogWarning(ex, "Realtime connection unavailable; falling back to manual refresh.");
            return false;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _gate.Dispose();
    }
}
