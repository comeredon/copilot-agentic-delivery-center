using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Catalog;

public sealed class BrewRecommendation : Entity
{
    private BrewRecommendation()
    {
        GrindDescriptor = string.Empty;
        Notes = string.Empty;
    }

    public required Guid CoffeeId { get; init; }

    public required BrewMethod Method { get; init; }

    public decimal DoseGrams { get; private set; }

    public decimal YieldGrams { get; private set; }

    public int WaterTemperatureC { get; private set; }

    public int BrewSeconds { get; private set; }

    public string GrindDescriptor { get; private set; }

    public string Notes { get; private set; }

    public decimal Ratio => Math.Round(YieldGrams / DoseGrams, 1, MidpointRounding.AwayFromZero);

    internal static BrewRecommendation Create(
        Guid coffeeId,
        BrewMethod method,
        decimal doseGrams,
        decimal yieldGrams,
        int waterTemperatureC,
        int brewSeconds,
        string grindDescriptor,
        string notes)
    {
        DomainException.ThrowIfNullOrWhiteSpace(grindDescriptor, nameof(grindDescriptor));

        if (doseGrams <= 0 || yieldGrams <= doseGrams || waterTemperatureC is < 70 or > 100 || brewSeconds <= 0)
        {
            throw new DomainException("The brew recipe contains an invalid dose, yield, temperature, or time.");
        }

        return new BrewRecommendation
        {
            CoffeeId = coffeeId,
            Method = method,
            DoseGrams = doseGrams,
            YieldGrams = yieldGrams,
            WaterTemperatureC = waterTemperatureC,
            BrewSeconds = brewSeconds,
            GrindDescriptor = grindDescriptor.Trim(),
            Notes = notes?.Trim() ?? string.Empty,
        };
    }
}
