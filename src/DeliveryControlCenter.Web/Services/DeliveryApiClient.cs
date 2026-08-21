using System.Net.Http.Json;
using DeliveryControlCenter.Contracts.Delivery;
using DeliveryControlCenter.Contracts.Operations;

namespace DeliveryControlCenter.Web.Services;

/// <summary>
/// The only way the UI reaches data. It never talks to the database or to GitHub directly,
/// which keeps every read on the audited, rate-limited API path.
/// </summary>
public sealed class DeliveryApiClient(HttpClient http, ILogger<DeliveryApiClient> logger)
{
    public Task<DeliveryOverviewDto?> GetOverviewAsync(CancellationToken ct = default) =>
        GetAsync<DeliveryOverviewDto>("api/delivery/overview", ct);

    public async Task<IReadOnlyList<WorkItemSummaryDto>> GetWorkItemsAsync(CancellationToken ct = default) =>
        await GetAsync<List<WorkItemSummaryDto>>("api/delivery/work-items", ct) ?? [];

    public Task<WorkItemDetailDto?> GetWorkItemAsync(Guid id, CancellationToken ct = default) =>
        GetAsync<WorkItemDetailDto>($"api/delivery/work-items/{id}", ct);

    public Task<SyncHealthDto?> GetSyncHealthAsync(CancellationToken ct = default) =>
        GetAsync<SyncHealthDto>("api/delivery/sync-health", ct);

    public Task<CacheHealthDto?> GetCacheHealthAsync(CancellationToken ct = default) =>
        GetAsync<CacheHealthDto>("api/delivery/cache-health", ct);

    public async Task<(bool Authorized, string Message)> AuthorizeAsync(Guid id, string actor, CancellationToken ct = default)
    {
        try
        {
            var response = await http.PostAsJsonAsync($"api/delivery/work-items/{id}/authorize", new { actor }, ct);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Authorized.");
            }

            return response.StatusCode switch
            {
                System.Net.HttpStatusCode.Forbidden => (false, "Only the repository owner can authorize this merge."),
                System.Net.HttpStatusCode.Conflict => (false, "The work item is not in a state that can be authorized."),
                System.Net.HttpStatusCode.TooManyRequests => (false, "Rate limit reached. Try again shortly."),
                _ => (false, "The authorization request failed."),
            };
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Authorization request failed.");
            return (false, "The API is unreachable.");
        }
    }

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        try
        {
            return await http.GetFromJsonAsync<T>(path, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // The UI degrades to an empty panel instead of an error page.
            logger.LogWarning(ex, "API read failed for {Path}.", path);
            return default;
        }
    }
}
