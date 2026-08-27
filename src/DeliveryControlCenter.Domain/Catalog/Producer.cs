using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Catalog;

public sealed class Producer : Entity
{
    private Producer()
    {
        Slug = string.Empty;
        Name = string.Empty;
        Country = string.Empty;
        Region = string.Empty;
        Story = string.Empty;
        ImageUrl = string.Empty;
        ImageAlt = string.Empty;
    }

    public string Slug { get; private set; }

    public string Name { get; private set; }

    public ProducerKind Kind { get; private set; }

    public string Country { get; private set; }

    public string Region { get; private set; }

    public string Story { get; private set; }

    public int ElevationMasl { get; private set; }

    public decimal? FarmSizeHectares { get; private set; }

    public int PartnerSince { get; private set; }

    public string ImageUrl { get; private set; }

    public string ImageAlt { get; private set; }

    public static Producer Create(
        string slug,
        string name,
        ProducerKind kind,
        string country,
        string region,
        string story,
        int elevationMasl,
        decimal? farmSizeHectares,
        int partnerSince,
        string imageUrl,
        string imageAlt,
        Guid? id = null)
    {
        DomainException.ThrowIfNullOrWhiteSpace(slug, nameof(slug));
        DomainException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        DomainException.ThrowIfNullOrWhiteSpace(country, nameof(country));
        DomainException.ThrowIfNullOrWhiteSpace(story, nameof(story));

        return new Producer
        {
            Id = id ?? Guid.CreateVersion7(),
            Slug = slug.Trim(),
            Name = name.Trim(),
            Kind = kind,
            Country = country.Trim(),
            Region = region?.Trim() ?? string.Empty,
            Story = story.Trim(),
            ElevationMasl = elevationMasl,
            FarmSizeHectares = farmSizeHectares,
            PartnerSince = partnerSince,
            ImageUrl = imageUrl?.Trim() ?? string.Empty,
            ImageAlt = imageAlt?.Trim() ?? string.Empty,
        };
    }
}
