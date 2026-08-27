using DeliveryControlCenter.Application.Catalog;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryControlCenter.Application;

public static class ApplicationServiceCollectionExtensions
{
    /// <summary>Registers use cases only. Ports are bound by the Infrastructure layer.</summary>
    public static IServiceCollection AddStoreApplication(this IServiceCollection services)
    {
        services.AddScoped<CatalogQueries>();
        return services;
    }
}
