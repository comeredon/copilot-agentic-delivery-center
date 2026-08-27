using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Application.Catalog;

namespace DeliveryControlCenter.UnitTests.Application;

public sealed class StoreCacheKeyTests
{
    [Fact]
    public void Catalog_DistinctDelimiterContainingFiltersNeverShareAKey()
    {
        var searchContainsDelimiter = StoreCacheKeys.Catalog(new CatalogFilter(Search: "a:b"));
        var countryContainsDelimiter = StoreCacheKeys.Catalog(new CatalogFilter(Search: "a", Country: "b:"));

        searchContainsDelimiter.ShouldNotBe(countryContainsDelimiter);
    }

    [Fact]
    public void Catalog_EquivalentNormalizedFiltersShareAStableKey()
    {
        var first = StoreCacheKeys.Catalog(new CatalogFilter(Search: " Ethiopia ", Sort: "SCORE", Page: 0, PageSize: 999));
        var second = StoreCacheKeys.Catalog(new CatalogFilter(Search: "Ethiopia", Sort: "score", Page: 1, PageSize: 48));

        first.ShouldBe(second);
        first.ShouldStartWith("ninth-parallel:v1:catalog:");
        first.Length.ShouldBeLessThan(100);
    }

    [Fact]
    public void Normalize_IsIdempotentAndClampsPaging()
    {
        var first = new CatalogFilter(Search: " Kenya ", Sort: "not-a-sort", Page: -4, PageSize: 500).Normalize();
        var second = first.Normalize();

        second.ShouldBe(first);
        first.Search.ShouldBe("Kenya");
        first.Sort.ShouldBe("featured");
        first.Page.ShouldBe(1);
        first.PageSize.ShouldBe(48);
    }

    [Fact]
    public void DetailKeys_AreCaseInsensitiveAndTypeSeparated()
    {
        StoreCacheKeys.Coffee("KII-AA").ShouldBe(StoreCacheKeys.Coffee("kii-aa"));
        StoreCacheKeys.Producer("KII-FACTORY").ShouldBe(StoreCacheKeys.Producer("kii-factory"));
        StoreCacheKeys.Coffee("kii-aa").ShouldNotBe(StoreCacheKeys.Producer("kii-aa"));
    }
}
