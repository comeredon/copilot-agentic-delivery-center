using System.Net;
using System.Net.Http.Json;
using DeliveryControlCenter.Contracts.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryControlCenter.IntegrationTests;

[Collection(nameof(ApiSuite))]
public sealed class CoffeeCatalogEndpointsTests(ApiFixture fixture)
{
    [Fact]
    public async Task Catalog_ReturnsTheDeterministicTwelveCoffeeRange()
    {
        var client = fixture.CreateClient();

        var catalog = await client.GetFromJsonAsync<CatalogPageDto>(
            "api/catalog?pageSize=48",
            TestContext.Current.CancellationToken);

        catalog.ShouldNotBeNull();
        catalog.Total.ShouldBe(12);
        catalog.Items.Select(item => item.Country).Distinct().Count().ShouldBe(10);
        catalog.Facets.Processes.ShouldNotBeEmpty();
        catalog.Facets.TastingNotes.ShouldContain(note => note.Value == "Jasmine");
    }

    [Fact]
    public async Task Catalog_FilterByCountryAndMinimumScore_ReturnsOnlyMatchingCoffees()
    {
        var client = fixture.CreateClient();

        var catalog = await client.GetFromJsonAsync<CatalogPageDto>(
            "api/catalog?country=Ethiopia&minimumScore=88",
            TestContext.Current.CancellationToken);

        catalog.ShouldNotBeNull();
        catalog.Items.ShouldNotBeEmpty();
        catalog.Items.ShouldAllBe(item => item.Country == "Ethiopia" && item.CupScore >= 88m);
        catalog.Facets.Countries.ShouldBe([new FacetValueDto("Ethiopia", catalog.Total)]);
    }

    [Fact]
    public async Task CoffeeDetail_ContainsTraceabilitySensoryDataAndBrewRecipes()
    {
        var client = fixture.CreateClient();

        var coffee = await client.GetFromJsonAsync<CoffeeDetailDto>(
            "api/catalog/kii-aa",
            TestContext.Current.CancellationToken);

        coffee.ShouldNotBeNull();
        coffee.Producer.Name.ShouldBe("Kii Factory");
        coffee.Origin.ElevationMaxMasl.ShouldBeGreaterThan(coffee.Origin.ElevationMinMasl);
        coffee.Sensory.TastingNotes.ShouldContain("Blackcurrant");
        coffee.Traceability.LotCode.ShouldNotBeNullOrWhiteSpace();
        coffee.BrewRecommendations.Count.ShouldBe(3);
        coffee.Variants.ShouldContain(variant => variant.BagSizeGrams == 1000);
    }

    [Fact]
    public async Task Catalog_InvalidProcess_ReturnsProblemDetails()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync(
            "api/catalog?process=magic",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
        problem.ShouldNotBeNull();
        problem.Status.ShouldBe((int)HttpStatusCode.BadRequest);
        problem.Detail.ShouldNotBeNull().ShouldContain("process");
    }

    [Fact]
    public async Task Catalog_SearchEscapesWildcardsAndTastingNoteIsCaseInsensitive()
    {
        var client = fixture.CreateClient();

        var wildcard = await client.GetFromJsonAsync<CatalogPageDto>(
            "api/catalog?search=%25_%25",
            TestContext.Current.CancellationToken);
        var tastingNote = await client.GetFromJsonAsync<CatalogPageDto>(
            "api/catalog?tastingNote=blackcurrant",
            TestContext.Current.CancellationToken);

        wildcard.ShouldNotBeNull();
        wildcard.Total.ShouldBe(0);
        tastingNote.ShouldNotBeNull();
        tastingNote.Items.ShouldContain(item => item.Slug == "kii-aa");
    }

    [Fact]
    public async Task Catalog_SortsAndPagesWithoutRepeatingItems()
    {
        var client = fixture.CreateClient();

        var first = await client.GetFromJsonAsync<CatalogPageDto>(
            "api/catalog?sort=price-asc&page=1&pageSize=4",
            TestContext.Current.CancellationToken);
        var second = await client.GetFromJsonAsync<CatalogPageDto>(
            "api/catalog?sort=price-asc&page=2&pageSize=4",
            TestContext.Current.CancellationToken);

        first.ShouldNotBeNull();
        second.ShouldNotBeNull();
        first.Total.ShouldBe(12);
        first.Items.Select(item => item.FromPriceUsd).ShouldBeInOrder(SortDirection.Ascending);
        first.Items.Select(item => item.Slug).Intersect(second.Items.Select(item => item.Slug)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Producer_ReturnsStoryAndCurrentLots()
    {
        var client = fixture.CreateClient();

        var producer = await client.GetFromJsonAsync<ProducerDetailDto>(
            "api/producers/halo-beriti",
            TestContext.Current.CancellationToken);

        producer.ShouldNotBeNull();
        producer.Producer.Name.ShouldBe("Halo Beriti");
        producer.Coffees.Count.ShouldBe(2);
    }

    [Theory]
    [InlineData("api/catalog/not-a-coffee")]
    [InlineData("api/producers/not-a-producer")]
    public async Task UnknownCatalogResources_Return404(string path)
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("api/catalog?sort=magic")]
    [InlineData("api/catalog?roastLevel=ultraviolet")]
    public async Task InvalidCatalogOptions_Return400(string path)
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
