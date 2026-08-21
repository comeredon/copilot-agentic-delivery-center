namespace DeliveryControlCenter.Application.Abstractions;

/// <summary>Cache keys are versioned so a shape change cannot serve stale payloads.</summary>
public static class CacheKeys
{
    private const string Version = "v1";

    public static string Overview => $"dcc:{Version}:overview";

    public static string WorkItemList => $"dcc:{Version}:workitems";

    public static string WorkItemDetail(Guid id) => $"dcc:{Version}:workitem:{id:N}";

    public static string SyncHealth => $"dcc:{Version}:sync-health";
}
