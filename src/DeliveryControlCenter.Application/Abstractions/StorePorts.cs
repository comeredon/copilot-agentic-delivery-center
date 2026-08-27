using DeliveryControlCenter.Application.Catalog;
using DeliveryControlCenter.Domain.Catalog;

namespace DeliveryControlCenter.Application.Abstractions;

public interface ICoffeeCatalogRepository
{
    Task<CatalogSearchResult> SearchAsync(CatalogFilter filter, CancellationToken cancellationToken);

    Task<Coffee?> FindActiveBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<Producer?> FindProducerBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<IReadOnlyList<Coffee>> ListActiveByProducerAsync(Guid producerId, CancellationToken cancellationToken);
}

public interface IStoreCache
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan localExpiration,
        TimeSpan distributedExpiration,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken);

    Task InvalidateAsync(string key, CancellationToken cancellationToken);

    Task InvalidateTagAsync(string tag, CancellationToken cancellationToken);
}
