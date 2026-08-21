using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Contracts.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace DeliveryControlCenter.Api.Realtime;

/// <summary>Server-to-client only. Clients receive change hints and refetch through the API.</summary>
public sealed class DeliveryHub : Hub
{
}

internal sealed class SignalRDeliveryNotifier(IHubContext<DeliveryHub> hub) : IDeliveryNotifier
{
    public Task NotifyAsync(string kind, Guid? workItemId, CancellationToken cancellationToken) =>
        hub.Clients.All.SendAsync(kind, DeliveryNotification.For(kind, workItemId), cancellationToken);
}
