using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Catalog;

public sealed class Coffee : Entity
{
    private readonly List<BrewRecommendation> _brewRecommendations = [];
    private readonly List<CoffeeDescriptor> _descriptors = [];
    private readonly List<CoffeeImage> _images = [];
    private readonly List<CoffeeVariant> _variants = [];

    private Coffee()
    {
        Slug = string.Empty;
        Name = string.Empty;
        ShortDescription = string.Empty;
        Story = string.Empty;
        Country = string.Empty;
        Region = string.Empty;
        Subregion = string.Empty;
        FarmName = string.Empty;
        DryingMethod = string.Empty;
        HarvestLabel = string.Empty;
        AcidityType = string.Empty;
        LotCode = string.Empty;
        ImportPartner = string.Empty;
    }

    public required Guid ProducerId { get; init; }

    public Producer? Producer { get; private set; }

    public string Slug { get; private set; }

    public string Name { get; private set; }

    public string ShortDescription { get; private set; }

    public string Story { get; private set; }

    public string Country { get; private set; }

    public string Region { get; private set; }

    public string Subregion { get; private set; }

    public string FarmName { get; private set; }

    public int ElevationMinMasl { get; private set; }

    public int ElevationMaxMasl { get; private set; }

    public CoffeeProcess Process { get; private set; }

    public int? FermentationHours { get; private set; }

    public string DryingMethod { get; private set; }

    public string HarvestLabel { get; private set; }

    public RoastLevel RoastLevel { get; private set; }

    public RoastIntent RoastIntent { get; private set; }

    public DateOnly RoastedOn { get; private set; }

    public int RestDays { get; private set; }

    public decimal CupScore { get; private set; }

    public int Acidity { get; private set; }

    public int Body { get; private set; }

    public int Sweetness { get; private set; }

    public string AcidityType { get; private set; }

    public string LotCode { get; private set; }

    public string ImportPartner { get; private set; }

    public decimal? FobPriceUsdPerLb { get; private set; }

    public CoffeeStatus Status { get; private set; } = CoffeeStatus.Draft;

    public IReadOnlyList<BrewRecommendation> BrewRecommendations => _brewRecommendations;

    public IReadOnlyList<CoffeeDescriptor> Descriptors => _descriptors;

    public IReadOnlyList<CoffeeImage> Images => _images;

    public IReadOnlyList<CoffeeVariant> Variants => _variants;

    public static Coffee Create(
        Guid producerId,
        string slug,
        string name,
        string shortDescription,
        string story,
        string country,
        string region,
        string subregion,
        string farmName,
        int elevationMinMasl,
        int elevationMaxMasl,
        CoffeeProcess process,
        int? fermentationHours,
        string dryingMethod,
        string harvestLabel,
        RoastLevel roastLevel,
        RoastIntent roastIntent,
        DateOnly roastedOn,
        int restDays,
        decimal cupScore,
        int acidity,
        int body,
        int sweetness,
        string acidityType,
        string lotCode,
        string importPartner,
        decimal? fobPriceUsdPerLb,
        Guid? id = null)
    {
        DomainException.ThrowIfNullOrWhiteSpace(slug, nameof(slug));
        DomainException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        DomainException.ThrowIfNullOrWhiteSpace(country, nameof(country));

        if (cupScore is < 60m or > 100m)
        {
            throw new DomainException("Cup score must be between 60 and 100.");
        }

        if (elevationMinMasl <= 0 || elevationMaxMasl < elevationMinMasl)
        {
            throw new DomainException("The coffee elevation range is invalid.");
        }

        if (process is CoffeeProcess.Anaerobic or CoffeeProcess.CarbonicMaceration or CoffeeProcess.YeastInoculated
            && fermentationHours is null or <= 0)
        {
            throw new DomainException("Fermented processes require a fermentation duration.");
        }

        if (acidity is < 1 or > 5 || body is < 1 or > 5 || sweetness is < 1 or > 5)
        {
            throw new DomainException("Sensory intensity values must be between one and five.");
        }

        return new Coffee
        {
            Id = id ?? Guid.CreateVersion7(),
            ProducerId = producerId,
            Slug = slug.Trim(),
            Name = name.Trim(),
            ShortDescription = shortDescription?.Trim() ?? string.Empty,
            Story = story?.Trim() ?? string.Empty,
            Country = country.Trim(),
            Region = region?.Trim() ?? string.Empty,
            Subregion = subregion?.Trim() ?? string.Empty,
            FarmName = farmName?.Trim() ?? string.Empty,
            ElevationMinMasl = elevationMinMasl,
            ElevationMaxMasl = elevationMaxMasl,
            Process = process,
            FermentationHours = fermentationHours,
            DryingMethod = dryingMethod?.Trim() ?? string.Empty,
            HarvestLabel = harvestLabel?.Trim() ?? string.Empty,
            RoastLevel = roastLevel,
            RoastIntent = roastIntent,
            RoastedOn = roastedOn,
            RestDays = restDays,
            CupScore = cupScore,
            Acidity = acidity,
            Body = body,
            Sweetness = sweetness,
            AcidityType = acidityType?.Trim() ?? string.Empty,
            LotCode = lotCode?.Trim() ?? string.Empty,
            ImportPartner = importPartner?.Trim() ?? string.Empty,
            FobPriceUsdPerLb = fobPriceUsdPerLb,
        };
    }

    public void AddDescriptor(DescriptorKind kind, string value)
    {
        if (_descriptors.Any(d => d.Kind == kind && string.Equals(d.Value, value, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"The {kind} '{value}' is already present.");
        }

        if (kind == DescriptorKind.TastingNote && _descriptors.Count(d => d.Kind == kind) >= 5)
        {
            throw new DomainException("A coffee can have at most five primary tasting notes.");
        }

        _descriptors.Add(CoffeeDescriptor.Create(Id, kind, value, _descriptors.Count(d => d.Kind == kind)));
        Touch();
    }

    public void AddVariant(string sku, int bagSizeGrams, GrindOption grind, decimal priceUsd, int stockOnHand)
    {
        if (_variants.Any(v => string.Equals(v.Sku, sku, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"The SKU '{sku}' is already present.");
        }

        _variants.Add(CoffeeVariant.Create(Id, sku, bagSizeGrams, grind, priceUsd, stockOnHand));
        Touch();
    }

    public void AddBrewRecommendation(
        BrewMethod method,
        decimal doseGrams,
        decimal yieldGrams,
        int waterTemperatureC,
        int brewSeconds,
        string grindDescriptor,
        string notes)
    {
        if (_brewRecommendations.Any(r => r.Method == method))
        {
            throw new DomainException($"A recipe for {method} is already present.");
        }

        _brewRecommendations.Add(BrewRecommendation.Create(
            Id,
            method,
            doseGrams,
            yieldGrams,
            waterTemperatureC,
            brewSeconds,
            grindDescriptor,
            notes));
        Touch();
    }

    public void AddImage(string url, string altText, bool isHero = false)
    {
        if (isHero && _images.Any(image => image.IsHero))
        {
            throw new DomainException("A coffee can have only one hero image.");
        }

        _images.Add(CoffeeImage.Create(Id, url, altText, isHero, _images.Count));
        Touch();
    }

    public void Publish()
    {
        if (!_images.Any(image => image.IsHero) || !_variants.Any(variant => variant.IsActive))
        {
            throw new DomainException("Publishing requires a hero image and at least one active variant.");
        }

        Status = CoffeeStatus.Active;
        Touch();
    }

    public void ReserveVariant(Guid variantId, int quantity)
    {
        var variant = _variants.FirstOrDefault(candidate => candidate.Id == variantId)
            ?? throw new DomainException("The requested coffee variant does not exist.");

        variant.Reserve(quantity);

        if (_variants.All(candidate => !candidate.IsActive || candidate.StockOnHand == 0))
        {
            Status = CoffeeStatus.SoldOut;
        }

        Touch();
    }

    public FreshnessState GetFreshnessState(DateOnly today)
    {
        var peakStarts = RoastedOn.AddDays(RestDays);
        if (today < peakStarts)
        {
            return FreshnessState.Resting;
        }

        return today <= peakStarts.AddDays(30) ? FreshnessState.Peak : FreshnessState.PastPeak;
    }
}
