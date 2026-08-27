using DeliveryControlCenter.Application;
using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Infrastructure.Caching;
using DeliveryControlCenter.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeliveryControlCenter.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public const string PostgresResourceName = "deliverydb";
    public const string RedisResourceName = "cache";

    /// <summary>
    /// Binds every application port to a concrete adapter. Hosts add their own notifier
    /// (SignalR in the API) after calling this.
    /// </summary>
    public static IHostApplicationBuilder AddStoreInfrastructure(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddNpgsqlDbContext<DeliveryDbContext>(PostgresResourceName);
        builder.AddRedisDistributedCache(connectionName: RedisResourceName);

#pragma warning disable EXTEXP0018 // HybridCache is stable in this configuration for our usage.
        builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

        builder.Services.AddScoped<ICoffeeCatalogRepository, CoffeeCatalogRepository>();

        builder.Services.AddSingleton<HybridDeliveryCache>();
        builder.Services.AddSingleton<IStoreCache>(services => services.GetRequiredService<HybridDeliveryCache>());

        builder.Services.AddStoreApplication();

        return builder;
    }

}
