using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Contracts.Catalog;
using DeliveryControlCenter.Domain.Catalog;

namespace DeliveryControlCenter.Application.Catalog;

public sealed class CatalogQueries(ICoffeeCatalogRepository repository, IStoreCache cache)
{
    private static readonly TimeSpan LocalExpiration = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan DistributedExpiration = TimeSpan.FromMinutes(5);

    public Task<CatalogPageDto> SearchAsync(CatalogFilter filter, CancellationToken cancellationToken)
    {
        var normalized = filter.Normalize();
        return cache.GetOrCreateAsync(
            StoreCacheKeys.Catalog(normalized),
            async ct =>
            {
                var result = await repository.SearchAsync(normalized, ct);
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                return new CatalogPageDto(
                    [.. result.Items.Select(coffee => coffee.ToCard(today))],
                    new CatalogFacetsDto(
                        MapFacets(result.Facets.Countries),
                        MapFacets(result.Facets.Processes),
                        MapFacets(result.Facets.RoastLevels),
                        MapFacets(result.Facets.TastingNotes)),
                    result.Total,
                    result.Page,
                    result.PageSize);
            },
            LocalExpiration,
            DistributedExpiration,
            [StoreCacheTags.Catalog],
            cancellationToken);
    }

    public Task<CoffeeDetailDto?> GetCoffeeAsync(string slug, CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(
            StoreCacheKeys.Coffee(slug),
            async ct =>
            {
                var coffee = await repository.FindActiveBySlugAsync(slug, ct);
                return coffee?.ToDetail(DateOnly.FromDateTime(DateTime.UtcNow));
            },
            LocalExpiration,
            DistributedExpiration,
            [StoreCacheTags.Catalog, StoreCacheTags.Coffee(slug)],
            cancellationToken);

    public Task<ProducerDetailDto?> GetProducerAsync(string slug, CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(
            StoreCacheKeys.Producer(slug),
            async ct =>
            {
                var producer = await repository.FindProducerBySlugAsync(slug, ct);
                if (producer is null)
                {
                    return null;
                }

                var coffees = await repository.ListActiveByProducerAsync(producer.Id, ct);
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                return new ProducerDetailDto(producer.ToSummary(), [.. coffees.Select(coffee => coffee.ToCard(today))]);
            },
            LocalExpiration,
            DistributedExpiration,
            [StoreCacheTags.Catalog, StoreCacheTags.Producer(slug)],
            cancellationToken);

    private static IReadOnlyList<FacetValueDto> MapFacets(IReadOnlyList<CatalogFacetValue> source) =>
        [.. source.Select(facet => new FacetValueDto(facet.Value, facet.Count))];
}
