using DeliveryControlCenter.Application.Delivery;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryControlCenter.Application;

public static class ApplicationServiceCollectionExtensions
{
    /// <summary>Registers use cases only. Ports are bound by the Infrastructure layer.</summary>
    public static IServiceCollection AddDeliveryApplication(this IServiceCollection services)
    {
        services.AddScoped<DeliveryQueries>();
        services.AddScoped<SynchronizeWorkItemsHandler>();
        services.AddScoped<RecordGateOutcomeHandler>();
        services.AddScoped<AuthorizeWorkItemHandler>();
        return services;
    }
}
