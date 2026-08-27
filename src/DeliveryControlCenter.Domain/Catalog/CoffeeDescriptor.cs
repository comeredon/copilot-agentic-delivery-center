using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Catalog;

public sealed class CoffeeDescriptor : Entity
{
    private CoffeeDescriptor()
    {
        Value = string.Empty;
    }

    public required Guid CoffeeId { get; init; }

    public required DescriptorKind Kind { get; init; }

    public string Value { get; private set; }

    public int SortOrder { get; private set; }

    internal static CoffeeDescriptor Create(Guid coffeeId, DescriptorKind kind, string value, int sortOrder)
    {
        DomainException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        return new CoffeeDescriptor
        {
            CoffeeId = coffeeId,
            Kind = kind,
            Value = value.Trim(),
            SortOrder = sortOrder,
        };
    }
}
