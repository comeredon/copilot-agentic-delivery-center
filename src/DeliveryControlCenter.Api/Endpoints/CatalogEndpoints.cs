using DeliveryControlCenter.Application.Catalog;
using DeliveryControlCenter.Contracts.Catalog;
using DeliveryControlCenter.Domain.Catalog;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DeliveryControlCenter.Api.Endpoints;

internal static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/catalog")
            .WithTags("Coffee catalog")
            .RequireRateLimiting(RateLimitPolicies.Read);

        group.MapGet("/", async Task<IResult> (
            string? search,
            string? country,
            string? process,
            string? roastLevel,
            string? tastingNote,
            decimal? minimumScore,
            decimal? maximumPrice,
            string? sort,
            int? page,
            int? pageSize,
            CatalogQueries queries,
            CancellationToken ct) =>
        {
            if (IsTooLong(search) || IsTooLong(country) || IsTooLong(tastingNote))
            {
                return TypedResults.Problem(
                    "Search, country, and tasting-note filters are limited to 128 characters.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!TryParseEnum<CoffeeProcess>(process, out var parsedProcess)
                || !TryParseEnum<RoastLevel>(roastLevel, out var parsedRoastLevel))
            {
                return TypedResults.Problem(
                    "The process or roast-level filter is not recognized.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!string.IsNullOrWhiteSpace(sort)
                && !CatalogFilter.AllowedSorts.Contains(sort.Trim().ToLowerInvariant()))
            {
                return TypedResults.Problem(
                    "The requested catalog sort is not recognized.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var filter = new CatalogFilter(
                search,
                country,
                parsedProcess,
                parsedRoastLevel,
                tastingNote,
                minimumScore,
                maximumPrice,
                sort ?? "featured",
                page ?? 1,
                pageSize ?? 12);

            return TypedResults.Ok(await queries.SearchAsync(filter, ct));
        })
        .WithName("SearchCoffeeCatalog")
        .Produces<CatalogPageDto>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{slug}", async Task<Results<Ok<CoffeeDetailDto>, NotFound>> (
            string slug,
            CatalogQueries queries,
            CancellationToken ct) =>
        {
            if (slug.Length > 128)
            {
                return TypedResults.NotFound();
            }

            var coffee = await queries.GetCoffeeAsync(slug, ct);
            return coffee is null ? TypedResults.NotFound() : TypedResults.Ok(coffee);
        })
        .WithName("GetCoffee")
        .Produces<CoffeeDetailDto>();

        app.MapGet("/api/producers/{slug}", async Task<Results<Ok<ProducerDetailDto>, NotFound>> (
            string slug,
            CatalogQueries queries,
            CancellationToken ct) =>
        {
            if (slug.Length > 128)
            {
                return TypedResults.NotFound();
            }

            var producer = await queries.GetProducerAsync(slug, ct);
            return producer is null ? TypedResults.NotFound() : TypedResults.Ok(producer);
        })
        .WithTags("Coffee producers")
        .WithName("GetCoffeeProducer")
        .RequireRateLimiting(RateLimitPolicies.Read)
        .Produces<ProducerDetailDto>();

        return app;
    }

    private static bool TryParseEnum<T>(string? value, out T? parsed)
        where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsed = null;
            return true;
        }

        if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
        {
            parsed = result;
            return true;
        }

        parsed = null;
        return false;
    }

    private static bool IsTooLong(string? value) => value?.Length > 128;
}
