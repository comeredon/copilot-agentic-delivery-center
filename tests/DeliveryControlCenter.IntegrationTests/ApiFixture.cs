using DeliveryControlCenter.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace DeliveryControlCenter.IntegrationTests;

/// <summary>
/// Boots the real API pipeline against throwaway Postgres and Redis containers. Nothing here
/// stubs the data layer, so these tests exercise the actual EF model and the actual cache.
/// </summary>
public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17.6")
        .WithDatabase("deliverydb")
        .WithUsername("dcc")
        .WithPassword("dcc-integration-tests")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:8.2")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
        await db.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:deliverydb", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:cache", _redis.GetConnectionString());
        builder.UseSetting("GitHub:Enabled", "false");
        builder.UseSetting("Delivery:RepositoryOwner", "comeredon");
        builder.UseSetting("Delivery:RepositoryName", "copilot-agentic-delivery-center");
        builder.UseEnvironment("Development");
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _redis.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    async ValueTask IAsyncDisposable.DisposeAsync() => await DisposeAsync();
}

[CollectionDefinition(nameof(ApiSuite))]
public sealed class ApiSuite : ICollectionFixture<ApiFixture>;
