using DeliveryControlCenter.Contracts.Catalog;

namespace DeliveryControlCenter.Web.Services;

public sealed class StoreCart
{
    private readonly List<CartLine> _lines = [];

    public event Action? Changed;

    public IReadOnlyList<CartLine> Lines => _lines;

    public int ItemCount => _lines.Sum(line => line.Quantity);

    public decimal Subtotal => _lines.Sum(line => line.UnitPriceUsd * line.Quantity);

    public bool Add(CoffeeDetailDto coffee, CoffeeVariantDto variant, int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(coffee);
        ArgumentNullException.ThrowIfNull(variant);

        var hero = coffee.Images.FirstOrDefault(image => image.IsHero);
        if (!variant.IsActive || variant.StockOnHand <= 0 || hero is null)
        {
            return false;
        }

        var existing = _lines.FirstOrDefault(line => line.VariantId == variant.Id);
        if (existing is not null)
        {
            existing.SetQuantity(Math.Min(existing.Quantity + quantity, Math.Min(10, variant.StockOnHand)));
        }
        else
        {
            _lines.Add(new CartLine(
                variant.Id,
                coffee.Slug,
                coffee.Name,
                coffee.Producer.Name,
                hero.Url,
                hero.AltText,
                variant.BagSizeGrams,
                variant.Grind,
                variant.PriceUsd,
                Math.Clamp(quantity, 1, Math.Min(10, variant.StockOnHand)),
                variant.StockOnHand));
        }

        Changed?.Invoke();
        return true;
    }

    public void SetQuantity(Guid variantId, int quantity)
    {
        var line = _lines.FirstOrDefault(candidate => candidate.VariantId == variantId);
        if (line is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            _lines.Remove(line);
        }
        else
        {
            line.SetQuantity(Math.Min(quantity, Math.Min(10, line.StockOnHand)));
        }

        Changed?.Invoke();
    }

    public void Remove(Guid variantId)
    {
        _lines.RemoveAll(line => line.VariantId == variantId);
        Changed?.Invoke();
    }
}

public sealed class CartLine(
    Guid variantId,
    string slug,
    string coffeeName,
    string producer,
    string imageUrl,
    string imageAlt,
    int bagSizeGrams,
    string grind,
    decimal unitPriceUsd,
    int quantity,
    int stockOnHand)
{
    public Guid VariantId { get; } = variantId;

    public string Slug { get; } = slug;

    public string CoffeeName { get; } = coffeeName;

    public string Producer { get; } = producer;

    public string ImageUrl { get; } = imageUrl;

    public string ImageAlt { get; } = imageAlt;

    public int BagSizeGrams { get; } = bagSizeGrams;

    public string Grind { get; } = grind;

    public decimal UnitPriceUsd { get; } = unitPriceUsd;

    public int Quantity { get; private set; } = quantity;

    public int StockOnHand { get; } = stockOnHand;

    public decimal LineTotal => UnitPriceUsd * Quantity;

    internal void SetQuantity(int value) => Quantity = value;
}
