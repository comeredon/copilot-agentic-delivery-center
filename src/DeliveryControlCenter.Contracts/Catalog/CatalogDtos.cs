namespace DeliveryControlCenter.Contracts.Catalog;

public sealed record CatalogPageDto(
    IReadOnlyList<CoffeeCardDto> Items,
    CatalogFacetsDto Facets,
    int Total,
    int Page,
    int PageSize);

public sealed record CoffeeCardDto(
    string Slug,
    string Name,
    string ShortDescription,
    string Country,
    string Region,
    string Producer,
    string Process,
    string RoastLevel,
    decimal CupScore,
    IReadOnlyList<string> TastingNotes,
    decimal FromPriceUsd,
    string HeroImageUrl,
    string HeroImageAlt,
    string Freshness,
    bool InStock);

public sealed record CoffeeDetailDto(
    string Slug,
    string Name,
    string ShortDescription,
    string Story,
    ProducerSummaryDto Producer,
    OriginDto Origin,
    ProcessingDto Processing,
    RoastDto Roast,
    SensoryDto Sensory,
    TraceabilityDto Traceability,
    IReadOnlyList<string> Varieties,
    IReadOnlyList<string> Certifications,
    IReadOnlyList<CoffeeVariantDto> Variants,
    IReadOnlyList<BrewRecommendationDto> BrewRecommendations,
    IReadOnlyList<CoffeeImageDto> Images);

public sealed record ProducerSummaryDto(
    string Slug,
    string Name,
    string Kind,
    string Country,
    string Region,
    string Story,
    int ElevationMasl,
    decimal? FarmSizeHectares,
    int PartnerSince,
    string ImageUrl,
    string ImageAlt);

public sealed record ProducerDetailDto(
    ProducerSummaryDto Producer,
    IReadOnlyList<CoffeeCardDto> Coffees);

public sealed record OriginDto(
    string Country,
    string Region,
    string Subregion,
    string FarmName,
    int ElevationMinMasl,
    int ElevationMaxMasl);

public sealed record ProcessingDto(
    string Method,
    int? FermentationHours,
    string DryingMethod,
    string HarvestLabel);

public sealed record RoastDto(
    string Level,
    string Intent,
    DateOnly RoastedOn,
    int RestDays,
    string Freshness);

public sealed record SensoryDto(
    decimal CupScore,
    int Acidity,
    int Body,
    int Sweetness,
    string AcidityType,
    IReadOnlyList<string> TastingNotes);

public sealed record TraceabilityDto(
    string LotCode,
    string ImportPartner,
    decimal? FobPriceUsdPerLb);

public sealed record CoffeeVariantDto(
    Guid Id,
    string Sku,
    int BagSizeGrams,
    string Grind,
    decimal PriceUsd,
    int StockOnHand,
    bool IsActive);

public sealed record BrewRecommendationDto(
    string Method,
    decimal DoseGrams,
    decimal YieldGrams,
    decimal Ratio,
    int WaterTemperatureC,
    int BrewSeconds,
    string GrindDescriptor,
    string Notes);

public sealed record CoffeeImageDto(
    string Url,
    string AltText,
    bool IsHero,
    int SortOrder);

public sealed record CatalogFacetsDto(
    IReadOnlyList<FacetValueDto> Countries,
    IReadOnlyList<FacetValueDto> Processes,
    IReadOnlyList<FacetValueDto> RoastLevels,
    IReadOnlyList<FacetValueDto> TastingNotes);

public sealed record FacetValueDto(string Value, int Count);
