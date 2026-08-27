using DeliveryControlCenter.Contracts.Catalog;
using DeliveryControlCenter.Domain.Catalog;

namespace DeliveryControlCenter.Application.Catalog;

internal static class CatalogMapping
{
    public static CoffeeCardDto ToCard(this Coffee coffee, DateOnly today)
    {
        var hero = coffee.Images.OrderBy(image => image.SortOrder).First(image => image.IsHero);
        var variants = coffee.Variants.Where(variant => variant.IsActive).ToList();

        return new CoffeeCardDto(
            coffee.Slug,
            coffee.Name,
            coffee.ShortDescription,
            coffee.Country,
            coffee.Region,
            coffee.Producer?.Name ?? coffee.FarmName,
            coffee.Process.ToString(),
            coffee.RoastLevel.ToString(),
            coffee.CupScore,
            [.. coffee.Descriptors
                .Where(descriptor => descriptor.Kind == DescriptorKind.TastingNote)
                .OrderBy(descriptor => descriptor.SortOrder)
                .Select(descriptor => descriptor.Value)],
            variants.Count == 0 ? 0m : variants.Min(variant => variant.PriceUsd),
            hero.Url,
            hero.AltText,
            coffee.GetFreshnessState(today).ToString(),
            variants.Any(variant => variant.StockOnHand > 0));
    }

    public static CoffeeDetailDto ToDetail(this Coffee coffee, DateOnly today)
    {
        var producer = coffee.Producer ?? throw new InvalidOperationException("Coffee detail requires a producer.");

        return new CoffeeDetailDto(
            coffee.Slug,
            coffee.Name,
            coffee.ShortDescription,
            coffee.Story,
            producer.ToSummary(),
            new OriginDto(
                coffee.Country,
                coffee.Region,
                coffee.Subregion,
                coffee.FarmName,
                coffee.ElevationMinMasl,
                coffee.ElevationMaxMasl),
            new ProcessingDto(
                coffee.Process.ToString(),
                coffee.FermentationHours,
                coffee.DryingMethod,
                coffee.HarvestLabel),
            new RoastDto(
                coffee.RoastLevel.ToString(),
                coffee.RoastIntent.ToString(),
                coffee.RoastedOn,
                coffee.RestDays,
                coffee.GetFreshnessState(today).ToString()),
            new SensoryDto(
                coffee.CupScore,
                coffee.Acidity,
                coffee.Body,
                coffee.Sweetness,
                coffee.AcidityType,
                Descriptors(coffee, DescriptorKind.TastingNote)),
            new TraceabilityDto(coffee.LotCode, coffee.ImportPartner, coffee.FobPriceUsdPerLb),
            Descriptors(coffee, DescriptorKind.Variety),
            Descriptors(coffee, DescriptorKind.Certification),
            [.. coffee.Variants
                .OrderBy(variant => variant.BagSizeGrams)
                .ThenBy(variant => variant.Grind)
                .Select(variant => new CoffeeVariantDto(
                    variant.Id,
                    variant.Sku,
                    variant.BagSizeGrams,
                    variant.Grind.ToString(),
                    variant.PriceUsd,
                    variant.StockOnHand,
                    variant.IsActive))],
            [.. coffee.BrewRecommendations
                .OrderBy(recipe => recipe.Method)
                .Select(recipe => new BrewRecommendationDto(
                    recipe.Method.ToString(),
                    recipe.DoseGrams,
                    recipe.YieldGrams,
                    recipe.Ratio,
                    recipe.WaterTemperatureC,
                    recipe.BrewSeconds,
                    recipe.GrindDescriptor,
                    recipe.Notes))],
            [.. coffee.Images
                .OrderBy(image => image.SortOrder)
                .Select(image => new CoffeeImageDto(image.Url, image.AltText, image.IsHero, image.SortOrder))]);
    }

    public static ProducerSummaryDto ToSummary(this Producer producer) => new(
        producer.Slug,
        producer.Name,
        producer.Kind.ToString(),
        producer.Country,
        producer.Region,
        producer.Story,
        producer.ElevationMasl,
        producer.FarmSizeHectares,
        producer.PartnerSince,
        producer.ImageUrl,
        producer.ImageAlt);

    private static IReadOnlyList<string> Descriptors(Coffee coffee, DescriptorKind kind) =>
        [.. coffee.Descriptors
            .Where(descriptor => descriptor.Kind == kind)
            .OrderBy(descriptor => descriptor.SortOrder)
            .Select(descriptor => descriptor.Value)];
}
