namespace DeliveryControlCenter.Contracts.Realtime;

/// <summary>Names shared by the API hub and the Blazor client so neither side hardcodes strings.</summary>
public static class DeliveryHubContract
{
    public const string Path = "/hubs/delivery";

    public const string OverviewChanged = nameof(OverviewChanged);
    public const string WorkItemChanged = nameof(WorkItemChanged);
    public const string SyncHealthChanged = nameof(SyncHealthChanged);
}

/// <summary>Envelope for every push. Consumers refetch by id rather than trusting pushed state.</summary>
public sealed record DeliveryNotification(string Kind, Guid? WorkItemId, DateTimeOffset RaisedAtUtc)
{
    public static DeliveryNotification For(string kind, Guid? workItemId = null) =>
        new(kind, workItemId, DateTimeOffset.UtcNow);
}
