using System.Net.Http.Json;
using DeliveryControlCenter.Contracts.Catalog;
using Microsoft.AspNetCore.WebUtilities;

namespace DeliveryControlCenter.Web.Services;

public sealed class CoffeeStoreApiClient(HttpClient http, ILogger<CoffeeStoreApiClient> logger)
{
    public Task<CatalogPageDto?> SearchAsync(
        string? search = null,
        string? country = null,
        string? process = null,
        string? roastLevel = null,
        string? tastingNote = null,
        decimal? minimumScore = null,
        decimal? maximumPrice = null,
        string sort = "featured",
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["search"] = search,
            ["country"] = country,
            ["process"] = process,
            ["roastLevel"] = roastLevel,
            ["tastingNote"] = tastingNote,
            ["minimumScore"] = minimumScore?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["maximumPrice"] = maximumPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["sort"] = sort,
            ["page"] = page.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["pageSize"] = pageSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };

        return GetAsync<CatalogPageDto>(QueryHelpers.AddQueryString("api/catalog", parameters), cancellationToken);
    }

    public Task<CoffeeDetailDto?> GetCoffeeAsync(string slug, CancellationToken cancellationToken = default) =>
        GetAsync<CoffeeDetailDto>($"api/catalog/{Uri.EscapeDataString(slug)}", cancellationToken);

    public Task<ProducerDetailDto?> GetProducerAsync(string slug, CancellationToken cancellationToken = default) =>
        GetAsync<ProducerDetailDto>($"api/producers/{Uri.EscapeDataString(slug)}", cancellationToken);

    private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        try
        {
            return await http.GetFromJsonAsync<T>(path, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Coffee store API read failed for {Path}.", path);
            return default;
        }
    }
}
