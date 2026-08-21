using System.Net;
using System.Net.Http.Json;
using DeliveryControlCenter.Application.Delivery;
using DeliveryControlCenter.Contracts.Delivery;
using DeliveryControlCenter.Contracts.Operations;
using DeliveryControlCenter.Domain.Sync;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryControlCenter.IntegrationTests;

[Collection(nameof(ApiSuite))]
public sealed class DeliveryEndpointsTests(ApiFixture fixture)
{
    [Fact]
    public async Task Overview_IsServedFromTheRealStack()
    {
        var client = fixture.CreateClient();

        var overview = await client.GetFromJsonAsync<DeliveryOverviewDto>("api/delivery/overview", TestContext.Current.CancellationToken);

        overview.ShouldNotBeNull();
        overview.Repository.ShouldBe("comeredon/copilot-agentic-delivery-center");
    }

    [Fact]
    public async Task SyncCycle_PersistsWorkItemsAndTheyAppearOnTheApi()
    {
        await RunSyncAsync();
        var client = fixture.CreateClient();

        var items = await client.GetFromJsonAsync<List<WorkItemSummaryDto>>("api/delivery/work-items", TestContext.Current.CancellationToken);

        items.ShouldNotBeNull();
        items.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task SecondSyncCycle_IsShortCircuitedByTheETag()
    {
        await RunSyncAsync();

        var second = await RunSyncAsync();

        second.ShouldBe(SyncOutcome.NotModified);
    }

    [Fact]
    public async Task Authorize_IsRefusedForANonOwner()
    {
        await RunSyncAsync();
        var client = fixture.CreateClient();
        var items = await client.GetFromJsonAsync<List<WorkItemSummaryDto>>("api/delivery/work-items", TestContext.Current.CancellationToken);
        var target = items!.First();

        var response = await client.PostAsJsonAsync(
            $"api/delivery/work-items/{target.Id}/authorize",
            new { actor = "delivery-orchestrator" },
            TestContext.Current.CancellationToken);

        // Non-owner is forbidden; an owner without a passing gate gets a conflict. Either way,
        // the agent cannot walk itself through the gate.
        response.StatusCode.ShouldBeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UnknownWorkItem_Returns404()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"api/delivery/work-items/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CacheHealth_ReportsCounters()
    {
        var client = fixture.CreateClient();
        await client.GetAsync("api/delivery/overview", TestContext.Current.CancellationToken);

        var health = await client.GetFromJsonAsync<CacheHealthDto>("api/delivery/cache-health", TestContext.Current.CancellationToken);

        health.ShouldNotBeNull();
        (health.Hits + health.Misses).ShouldBeGreaterThan(0);
    }

    private async Task<SyncOutcome> RunSyncAsync()
    {
        using var scope = fixture.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<SynchronizeWorkItemsHandler>();
        return await handler.HandleAsync(SyncFeed.Issues, TestContext.Current.CancellationToken);
    }
}
