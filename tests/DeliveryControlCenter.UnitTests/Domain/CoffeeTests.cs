using DeliveryControlCenter.Domain.Catalog;
using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.UnitTests.Domain;

public sealed class CoffeeTests
{
    [Theory]
    [InlineData(59.9)]
    [InlineData(100.1)]
    public void Create_RejectsCupScoresOutsideTheCuppingScale(decimal score)
    {
        Should.Throw<DomainException>(() => CreateCoffee(score));
    }

    [Theory]
    [InlineData(60)]
    [InlineData(100)]
    public void Create_AcceptsCupScoreBoundaries(decimal score)
    {
        CreateCoffee(score).CupScore.ShouldBe(score);
    }

    [Fact]
    public void Create_RejectsAnInvertedElevationRange()
    {
        Should.Throw<DomainException>(() => CreateCoffee(elevationMin: 2100, elevationMax: 1900));
    }

    [Fact]
    public void Create_FermentedProcessRequiresDuration()
    {
        Should.Throw<DomainException>(() => CreateCoffee(process: CoffeeProcess.Anaerobic));
        CreateCoffee(process: CoffeeProcess.Anaerobic, fermentationHours: 48).FermentationHours.ShouldBe(48);
    }

    [Theory]
    [InlineData(0, 3, 3)]
    [InlineData(3, 6, 3)]
    [InlineData(3, 3, 9)]
    public void Create_RejectsSensoryIntensityOutsideOneToFive(int acidity, int body, int sweetness)
    {
        Should.Throw<DomainException>(() => CreateCoffee(acidity: acidity, body: body, sweetness: sweetness));
    }

    [Fact]
    public void Publish_RequiresAHeroImageAndAnActiveVariant()
    {
        var coffee = CreateCoffee();

        Should.Throw<DomainException>(coffee.Publish);

        coffee.AddImage("https://example.test/coffee.jpg", "Coffee cup", isHero: true);
        Should.Throw<DomainException>(coffee.Publish);

        coffee.AddVariant("TEST-250", 250, GrindOption.WholeBean, 24m, 10);
        coffee.Publish();

        coffee.Status.ShouldBe(CoffeeStatus.Active);
    }

    [Fact]
    public void AddDescriptor_RejectsDuplicateAndMoreThanFiveTastingNotes()
    {
        var coffee = CreateCoffee();
        coffee.AddDescriptor(DescriptorKind.TastingNote, "Jasmine");

        Should.Throw<DomainException>(() => coffee.AddDescriptor(DescriptorKind.TastingNote, "jasmine"));

        coffee.AddDescriptor(DescriptorKind.TastingNote, "Peach");
        coffee.AddDescriptor(DescriptorKind.TastingNote, "Bergamot");
        coffee.AddDescriptor(DescriptorKind.TastingNote, "Honey");
        coffee.AddDescriptor(DescriptorKind.TastingNote, "Tea");

        Should.Throw<DomainException>(() => coffee.AddDescriptor(DescriptorKind.TastingNote, "Lemon"));
    }

    [Fact]
    public void Reserve_RefusesOversellingAndMarksTheVariantInactiveAtZero()
    {
        var coffee = CreateCoffee();
        coffee.AddVariant("TEST-250", 250, GrindOption.WholeBean, 24m, 2);
        var variant = coffee.Variants.Single();

        Should.Throw<DomainException>(() => coffee.ReserveVariant(variant.Id, 3));
        coffee.ReserveVariant(variant.Id, 2);

        variant.StockOnHand.ShouldBe(0);
        variant.IsActive.ShouldBeFalse();
        coffee.Status.ShouldBe(CoffeeStatus.SoldOut);
    }

    [Fact]
    public void GetFreshnessState_UsesTheConfiguredRestWindow()
    {
        var coffee = CreateCoffee(roastedOn: new DateOnly(2026, 8, 20), restDays: 10);

        coffee.GetFreshnessState(new DateOnly(2026, 8, 24)).ShouldBe(FreshnessState.Resting);
        coffee.GetFreshnessState(new DateOnly(2026, 9, 5)).ShouldBe(FreshnessState.Peak);
        coffee.GetFreshnessState(new DateOnly(2026, 10, 5)).ShouldBe(FreshnessState.PastPeak);
    }

    [Fact]
    public void BrewRecommendation_ComputesAStableRatio()
    {
        var coffee = CreateCoffee();
        coffee.AddBrewRecommendation(BrewMethod.V60, 15m, 250m, 94, 180, "Medium-fine", "Three pours");

        coffee.BrewRecommendations.Single().Ratio.ShouldBe(16.7m);
    }

    [Fact]
    public void AddVariant_RejectsInvalidBagPriceStockAndDuplicateSku()
    {
        var coffee = CreateCoffee();

        Should.Throw<DomainException>(() => coffee.AddVariant("BAD-SIZE", 300, GrindOption.WholeBean, 20m, 1));
        Should.Throw<DomainException>(() => coffee.AddVariant("BAD-PRICE", 250, GrindOption.WholeBean, 0m, 1));
        Should.Throw<DomainException>(() => coffee.AddVariant("BAD-STOCK", 250, GrindOption.WholeBean, 20m, -1));

        coffee.AddVariant("VALID", 250, GrindOption.WholeBean, 20m, 1);
        Should.Throw<DomainException>(() => coffee.AddVariant("valid", 500, GrindOption.WholeBean, 30m, 1));
    }

    [Fact]
    public void AddBrewRecommendation_RejectsDuplicateMethodAndInvalidRecipe()
    {
        var coffee = CreateCoffee();

        Should.Throw<DomainException>(() =>
            coffee.AddBrewRecommendation(BrewMethod.V60, 15m, 10m, 94, 180, "Fine", "Invalid yield"));

        coffee.AddBrewRecommendation(BrewMethod.V60, 15m, 250m, 94, 180, "Medium-fine", "Valid");
        Should.Throw<DomainException>(() =>
            coffee.AddBrewRecommendation(BrewMethod.V60, 18m, 300m, 94, 200, "Medium-fine", "Duplicate"));
    }

    [Fact]
    public void AddImage_RejectsASecondHero()
    {
        var coffee = CreateCoffee();
        coffee.AddImage("hero-one.jpg", "First hero", isHero: true);

        Should.Throw<DomainException>(() => coffee.AddImage("hero-two.jpg", "Second hero", isHero: true));
    }

    [Fact]
    public void Producer_CreateRequiresIdentityAndOrigin()
    {
        Should.Throw<DomainException>(() => Producer.Create(
            "",
            "Producer",
            ProducerKind.Farm,
            "Kenya",
            "Kirinyaga",
            "Story",
            1700,
            null,
            2020,
            "image.jpg",
            "Producer"));
    }

    private static Coffee CreateCoffee(
        decimal score = 88.4m,
        DateOnly? roastedOn = null,
        int restDays = 10,
        int elevationMin = 1900,
        int elevationMax = 2100,
        CoffeeProcess process = CoffeeProcess.Washed,
        int? fermentationHours = null,
        int acidity = 5,
        int body = 2,
        int sweetness = 4) =>
        Coffee.Create(
            Guid.CreateVersion7(),
            "test-coffee",
            "Test Coffee",
            "Jasmine and peach.",
            "A test lot.",
            "Ethiopia",
            "Gedeo",
            "Gedeb",
            "Test Farm",
            elevationMin,
            elevationMax,
            process,
            fermentationHours,
            "Raised beds",
            "2026",
            RoastLevel.Light,
            RoastIntent.Filter,
            roastedOn ?? new DateOnly(2026, 8, 20),
            restDays,
            score,
            acidity,
            body,
            sweetness,
            "Citric",
            "TEST-LOT",
            "Test Importer",
            4.25m);
}
