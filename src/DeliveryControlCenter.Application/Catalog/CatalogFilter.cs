using DeliveryControlCenter.Domain.Catalog;

namespace DeliveryControlCenter.Application.Catalog;

public sealed record CatalogFilter(
    string? Search = null,
    string? Country = null,
    CoffeeProcess? Process = null,
    RoastLevel? RoastLevel = null,
    string? TastingNote = null,
    decimal? MinimumScore = null,
    decimal? MaximumPrice = null,
    string Sort = "featured",
    int Page = 1,
    int PageSize = 12)
{
    public static IReadOnlySet<string> AllowedSorts { get; } =
        new HashSet<string>(["featured", "price-asc", "price-desc", "score", "newest"], StringComparer.Ordinal);

    public CatalogFilter Normalize() => this with
    {
        Search = Search?.Trim(),
        Country = Country?.Trim(),
        TastingNote = TastingNote?.Trim(),
        Sort = NormalizeSort(Sort),
        Page = Math.Max(1, Page),
        PageSize = Math.Clamp(PageSize, 1, 48),
    };

    private static string NormalizeSort(string? sort)
    {
        var normalized = string.IsNullOrWhiteSpace(sort) ? "featured" : sort.Trim().ToLowerInvariant();
        return AllowedSorts.Contains(normalized) ? normalized : "featured";
    }
}

public sealed record CatalogSearchResult(
    IReadOnlyList<Coffee> Items,
    CatalogFacetResult Facets,
    int Total,
    int Page,
    int PageSize);

public sealed record CatalogFacetResult(
    IReadOnlyList<CatalogFacetValue> Countries,
    IReadOnlyList<CatalogFacetValue> Processes,
    IReadOnlyList<CatalogFacetValue> RoastLevels,
    IReadOnlyList<CatalogFacetValue> TastingNotes);

public sealed record CatalogFacetValue(string Value, int Count);
