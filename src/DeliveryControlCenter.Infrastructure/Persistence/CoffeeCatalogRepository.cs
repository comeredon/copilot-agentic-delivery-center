using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Application.Catalog;
using DeliveryControlCenter.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace DeliveryControlCenter.Infrastructure.Persistence;

internal sealed class CoffeeCatalogRepository(DeliveryDbContext db) : ICoffeeCatalogRepository
{
    public async Task<CatalogSearchResult> SearchAsync(CatalogFilter filter, CancellationToken cancellationToken)
    {
        var normalized = filter.Normalize();
        var query = ActiveQuery();

        if (!string.IsNullOrWhiteSpace(normalized.Search))
        {
            var pattern = $"%{EscapeLikePattern(normalized.Search)}%";
            query = query.Where(coffee =>
                EF.Functions.ILike(coffee.Name, pattern, "\\")
                || EF.Functions.ILike(coffee.Country, pattern, "\\")
                || EF.Functions.ILike(coffee.Region, pattern, "\\")
                || coffee.Descriptors.Any(descriptor => EF.Functions.ILike(descriptor.Value, pattern, "\\")));
        }

        if (!string.IsNullOrWhiteSpace(normalized.Country))
        {
            query = query.Where(coffee => EF.Functions.ILike(coffee.Country, EscapeLikePattern(normalized.Country), "\\"));
        }

        if (normalized.Process is not null)
        {
            query = query.Where(coffee => coffee.Process == normalized.Process);
        }

        if (normalized.RoastLevel is not null)
        {
            query = query.Where(coffee => coffee.RoastLevel == normalized.RoastLevel);
        }

        if (!string.IsNullOrWhiteSpace(normalized.TastingNote))
        {
            query = query.Where(coffee => coffee.Descriptors.Any(descriptor =>
                descriptor.Kind == DescriptorKind.TastingNote
                && EF.Functions.ILike(descriptor.Value, EscapeLikePattern(normalized.TastingNote), "\\")));
        }

        if (normalized.MinimumScore is not null)
        {
            query = query.Where(coffee => coffee.CupScore >= normalized.MinimumScore);
        }

        if (normalized.MaximumPrice is not null)
        {
            query = query.Where(coffee => coffee.Variants.Any(variant =>
                variant.IsActive && variant.PriceUsd <= normalized.MaximumPrice));
        }

        var facets = await BuildFacetsAsync(query, cancellationToken);
        var ordered = normalized.Sort switch
        {
            "price-asc" => query.OrderBy(coffee => coffee.Variants
                .Where(variant => variant.IsActive)
                .Min(variant => variant.PriceUsd)),
            "price-desc" => query.OrderByDescending(coffee => coffee.Variants
                .Where(variant => variant.IsActive)
                .Min(variant => variant.PriceUsd)),
            "score" => query.OrderByDescending(coffee => coffee.CupScore),
            "newest" => query.OrderByDescending(coffee => coffee.RoastedOn),
            _ => query.OrderByDescending(coffee => coffee.CupScore).ThenBy(coffee => coffee.Name),
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await WithGraph(ordered)
            .Skip((normalized.Page - 1) * normalized.PageSize)
            .Take(normalized.PageSize)
            .ToListAsync(cancellationToken);

        return new CatalogSearchResult(items, facets, total, normalized.Page, normalized.PageSize);
    }

    public Task<Coffee?> FindActiveBySlugAsync(string slug, CancellationToken cancellationToken) =>
        WithGraph(ActiveQuery()).FirstOrDefaultAsync(
            coffee => coffee.Slug == slug,
            cancellationToken);

    public Task<Producer?> FindProducerBySlugAsync(string slug, CancellationToken cancellationToken) =>
        db.Producers.AsNoTracking().FirstOrDefaultAsync(producer => producer.Slug == slug, cancellationToken);

    public async Task<IReadOnlyList<Coffee>> ListActiveByProducerAsync(Guid producerId, CancellationToken cancellationToken) =>
        await WithGraph(ActiveQuery())
            .Where(coffee => coffee.ProducerId == producerId)
            .OrderByDescending(coffee => coffee.CupScore)
            .ToListAsync(cancellationToken);

    private IQueryable<Coffee> ActiveQuery() => db.Coffees
        .AsNoTracking()
        .Where(coffee => coffee.Status == CoffeeStatus.Active
                         && coffee.Variants.Any(variant => variant.IsActive && variant.StockOnHand > 0));

    private static IQueryable<Coffee> WithGraph(IQueryable<Coffee> query) => query
        .AsSplitQuery()
        .Include(coffee => coffee.Producer)
        .Include(coffee => coffee.Descriptors)
        .Include(coffee => coffee.Variants)
        .Include(coffee => coffee.BrewRecommendations)
        .Include(coffee => coffee.Images);

    private async Task<CatalogFacetResult> BuildFacetsAsync(
        IQueryable<Coffee> filtered,
        CancellationToken cancellationToken)
    {
        var countryGroups = await filtered
            .GroupBy(coffee => coffee.Country)
            .Select(group => new { Value = group.Key, Count = group.Count() })
            .OrderBy(facet => facet.Value)
            .ToListAsync(cancellationToken);

        var processGroups = await filtered
            .GroupBy(coffee => coffee.Process)
            .Select(group => new { group.Key, Count = group.Count() })
            .OrderBy(facet => facet.Key)
            .ToListAsync(cancellationToken);

        var roastGroups = await filtered
            .GroupBy(coffee => coffee.RoastLevel)
            .Select(group => new { group.Key, Count = group.Count() })
            .OrderBy(facet => facet.Key)
            .ToListAsync(cancellationToken);

        var filteredIds = filtered.Select(coffee => coffee.Id);
        var tastingNoteGroups = await db.CoffeeDescriptors
            .AsNoTracking()
            .Where(descriptor => descriptor.Kind == DescriptorKind.TastingNote
                                 && filteredIds.Contains(descriptor.CoffeeId))
            .GroupBy(descriptor => descriptor.Value)
            .Select(group => new { Value = group.Key, Count = group.Count() })
            .OrderBy(facet => facet.Value)
            .ToListAsync(cancellationToken);

        return new CatalogFacetResult(
            [.. countryGroups.Select(group => new CatalogFacetValue(group.Value, group.Count))],
            [.. processGroups.Select(group => new CatalogFacetValue(group.Key.ToString(), group.Count))],
            [.. roastGroups.Select(group => new CatalogFacetValue(group.Key.ToString(), group.Count))],
            [.. tastingNoteGroups.Select(group => new CatalogFacetValue(group.Value, group.Count))]);
    }

    private static string EscapeLikePattern(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal);
}
