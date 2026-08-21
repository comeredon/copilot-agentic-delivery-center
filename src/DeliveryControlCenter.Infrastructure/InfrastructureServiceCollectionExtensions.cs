using System.Net.Http.Headers;
using DeliveryControlCenter.Application;
using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Application.Configuration;
using DeliveryControlCenter.Infrastructure.Caching;
using DeliveryControlCenter.Infrastructure.GitHub;
using DeliveryControlCenter.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    public static IHostApplicationBuilder AddDeliveryInfrastructure(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptions<DeliveryOptions>()
            .Bind(builder.Configuration.GetSection(DeliveryOptions.SectionName))
            .Validate(o => o.SyncInterval > TimeSpan.Zero, "Delivery:SyncInterval must be positive.")
            .ValidateOnStart();

        builder.Services.AddOptions<GitHubOptions>()
            .Bind(builder.Configuration.GetSection(GitHubOptions.SectionName))
            .ValidateOnStart();

        builder.AddNpgsqlDbContext<DeliveryDbContext>(PostgresResourceName);
        builder.AddRedisDistributedCache(connectionName: RedisResourceName);

#pragma warning disable EXTEXP0018 // HybridCache is stable in this configuration for our usage.
        builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

        builder.Services.AddScoped<IWorkItemRepository, WorkItemRepository>();
        builder.Services.AddScoped<IGateReportRepository, GateReportRepository>();
        builder.Services.AddScoped<IGateLedgerRepository, GateLedgerRepository>();
        builder.Services.AddScoped<IAgentEvidenceRepository, AgentEvidenceRepository>();
        builder.Services.AddScoped<ISyncCursorRepository, SyncCursorRepository>();
        builder.Services.AddScoped<IAuditLog, AuditLog>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddSingleton<IDeliveryCache, HybridDeliveryCache>();
        builder.Services.AddScoped<IProjectWriter, GuardedProjectWriter>();
        builder.Services.TryAddSingleton<IDeliveryNotifier, NullDeliveryNotifier>();

        builder.AddGitHubReader();
        builder.Services.AddDeliveryApplication();

        return builder;
    }

    private static void AddGitHubReader(this IHostApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(GitHubOptions.SectionName);
        var enabled = section.GetValue<bool>(nameof(GitHubOptions.Enabled));

        if (!enabled)
        {
            builder.Services.AddSingleton<IGitHubReader, SeededGitHubReader>();
            return;
        }

        // Token comes from user-secrets or the environment; it is never written to configuration files.
        var token = builder.Configuration["GitHub:Token"];
        builder.Services.AddSingleton<IGitHubTokenProvider>(_ => new StaticGitHubTokenProvider(token));

        builder.Services.AddHttpClient<IGitHubReader, GitHubRestReader>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GitHubOptions>>().Value;
            client.BaseAddress = options.BaseAddress;
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("delivery-control-center", "1.0"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler();
    }
}
