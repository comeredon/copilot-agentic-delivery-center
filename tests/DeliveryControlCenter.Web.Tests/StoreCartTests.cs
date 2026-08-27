using DeliveryControlCenter.Contracts.Catalog;
using DeliveryControlCenter.Web.Services;

namespace DeliveryControlCenter.WebTests;

public sealed class StoreCartTests
{
    [Fact]
    public void Add_SameVariantTwice_MergesAndCapsAtStockOrTen()
    {
        var cart = new StoreCart();
        var coffee = Coffee();
        var variant = Variant(stock: 7);

        cart.Add(coffee, variant, 5).ShouldBeTrue();
        cart.Add(coffee, variant, 5).ShouldBeTrue();

        var line = cart.Lines.Single();
        line.Quantity.ShouldBe(7);
        cart.ItemCount.ShouldBe(7);
        cart.Subtotal.ShouldBe(182m);
    }

    [Fact]
    public void Add_OutOfStockVariant_IsRejectedWithoutChangingTheCart()
    {
        var cart = new StoreCart();

        cart.Add(Coffee(), Variant(stock: 0, active: false)).ShouldBeFalse();

        cart.Lines.ShouldBeEmpty();
    }

    [Fact]
    public void Add_CoffeeWithoutAHeroImage_IsRejectedWithoutThrowing()
    {
        var cart = new StoreCart();
        var coffee = Coffee() with { Images = [] };

        cart.Add(coffee, Variant()).ShouldBeFalse();
        cart.Lines.ShouldBeEmpty();
    }

    [Fact]
    public void SetQuantity_AtZero_RemovesTheLine()
    {
        var cart = new StoreCart();
        var variant = Variant();
        cart.Add(Coffee(), variant);

        cart.SetQuantity(variant.Id, 0);

        cart.Lines.ShouldBeEmpty();
        cart.Subtotal.ShouldBe(0m);
    }

    [Fact]
    public void Remove_UnknownVariant_IsANoOpButStillNotifiesObservers()
    {
        var cart = new StoreCart();
        var notifications = 0;
        cart.Changed += () => notifications++;

        cart.Remove(Guid.CreateVersion7());

        notifications.ShouldBe(1);
    }

    private static CoffeeVariantDto Variant(int stock = 10, bool active = true) =>
        new(Guid.Parse("30000000-0000-7000-8000-000000000001"), "KII-250-WB", 250, "WholeBean", 26m, stock, active);

    private static CoffeeDetailDto Coffee() => new(
        "kii-aa",
        "Kii AA",
        "Blackcurrant and grapefruit.",
        "A traceable Kenyan lot.",
        new ProducerSummaryDto("kii-factory", "Kii Factory", "WashingStation", "Kenya", "Kirinyaga", "Producer story", 1650, null, 2018, "producer.jpg", "Kii Factory"),
        new OriginDto("Kenya", "Kirinyaga", "Ngariama", "Kii Factory", 1600, 1800),
        new ProcessingDto("Washed", null, "Raised beds", "2025"),
        new RoastDto("Light", "Filter", new DateOnly(2026, 8, 17), 12, "Peak"),
        new SensoryDto(88.7m, 5, 3, 4, "Phosphoric", ["Blackcurrant"]),
        new TraceabilityDto("KE-KII", "Sucafina", 4.60m),
        ["SL28"],
        ["Rainforest Alliance"],
        [Variant()],
        [],
        [new CoffeeImageDto("coffee.jpg", "Kii AA coffee", true, 0)]);
}
