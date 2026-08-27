using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Catalog;

public sealed class CoffeeVariant : Entity
{
    private CoffeeVariant()
    {
        Sku = string.Empty;
    }

    public required Guid CoffeeId { get; init; }

    public string Sku { get; private set; }

    public int BagSizeGrams { get; private set; }

    public GrindOption Grind { get; private set; }

    public decimal PriceUsd { get; private set; }

    public int StockOnHand { get; private set; }

    public bool IsActive { get; private set; } = true;

    internal static CoffeeVariant Create(
        Guid coffeeId,
        string sku,
        int bagSizeGrams,
        GrindOption grind,
        decimal priceUsd,
        int stockOnHand)
    {
        DomainException.ThrowIfNullOrWhiteSpace(sku, nameof(sku));

        if (bagSizeGrams is not (250 or 500 or 1000))
        {
            throw new DomainException("Coffee bags must be 250 g, 500 g, or 1 kg.");
        }

        if (priceUsd <= 0 || stockOnHand < 0)
        {
            throw new DomainException("Price must be positive and stock cannot be negative.");
        }

        return new CoffeeVariant
        {
            CoffeeId = coffeeId,
            Sku = sku.Trim(),
            BagSizeGrams = bagSizeGrams,
            Grind = grind,
            PriceUsd = priceUsd,
            StockOnHand = stockOnHand,
        };
    }

    internal void Reserve(int quantity)
    {
        if (quantity <= 0 || quantity > StockOnHand)
        {
            throw new DomainException("The requested quantity is not available.");
        }

        StockOnHand -= quantity;
        IsActive = StockOnHand > 0;
        Touch();
    }
}
