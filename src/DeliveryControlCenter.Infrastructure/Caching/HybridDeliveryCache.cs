using DeliveryControlCenter.Application.Abstractions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace DeliveryControlCenter.Infrastructure.Caching;

/// <summary>
/// Cache-aside over <see cref="HybridCache"/>: in-process L1 plus Redis L2, with stampede
/// protection provided by the underlying implementation. Counters are exposed so the UI can
/// show hit ratio and prove invalidation actually happens.
/// </summary>
internal sealed class HybridDeliveryCache(HybridCache cache, ILogger<HybridDeliveryCache> logger) : IStoreCache
{
    private long _hits;
    private long _misses;
    private long _invalidations;
    private long _errors;

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        TimeSpan localExpiration,
        TimeSpan distributedExpiration,
        IReadOnlyCollection<string> tags,
        CancellationToken cancellationToken)
    {
        var produced = false;

        try
        {
            var value = await cache.GetOrCreateAsync(
                key,
                async ct =>
                {
                    produced = true;
                    return await factory(ct);
                },
                new HybridCacheEntryOptions
                {
                    LocalCacheExpiration = localExpiration,
                    Expiration = distributedExpiration,
                },
                tags,
                cancellationToken: cancellationToken);

            if (produced)
            {
                Interlocked.Increment(ref _misses);
            }
            else
            {
                Interlocked.Increment(ref _hits);
            }

            return value;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // A cache outage must degrade to a direct read, never fail the request.
            Interlocked.Increment(ref _errors);
            logger.LogWarning(ex, "Cache read failed for {Key}; falling back to the source.", key);
            return await factory(cancellationToken);
        }
    }

    public async Task InvalidateAsync(string key, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _invalidations);
        await cache.RemoveAsync(key, cancellationToken);
    }

    public async Task InvalidateTagAsync(string tag, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _invalidations);
        await cache.RemoveByTagAsync(tag, cancellationToken);
    }

}
