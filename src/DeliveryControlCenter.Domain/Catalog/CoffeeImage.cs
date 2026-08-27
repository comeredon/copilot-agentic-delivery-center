using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Catalog;

public sealed class CoffeeImage : Entity
{
    private CoffeeImage()
    {
        Url = string.Empty;
        AltText = string.Empty;
    }

    public required Guid CoffeeId { get; init; }

    public string Url { get; private set; }

    public string AltText { get; private set; }

    public bool IsHero { get; private set; }

    public int SortOrder { get; private set; }

    internal static CoffeeImage Create(Guid coffeeId, string url, string altText, bool isHero, int sortOrder)
    {
        DomainException.ThrowIfNullOrWhiteSpace(url, nameof(url));
        DomainException.ThrowIfNullOrWhiteSpace(altText, nameof(altText));

        return new CoffeeImage
        {
            CoffeeId = coffeeId,
            Url = url.Trim(),
            AltText = altText.Trim(),
            IsHero = isHero,
            SortOrder = sortOrder,
        };
    }
}
