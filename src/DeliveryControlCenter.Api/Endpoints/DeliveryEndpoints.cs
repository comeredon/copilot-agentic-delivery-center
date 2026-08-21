using DeliveryControlCenter.Application.Configuration;
using DeliveryControlCenter.Application.Delivery;
using DeliveryControlCenter.Contracts.Delivery;
using DeliveryControlCenter.Contracts.Operations;
using DeliveryControlCenter.Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DeliveryControlCenter.Api.Endpoints;

internal static class DeliveryEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/delivery")
            .WithTags("Delivery")
            .RequireRateLimiting(RateLimitPolicies.Read);

        group.MapGet("/overview", async (DeliveryQueries queries, CancellationToken ct) =>
                TypedResults.Ok(await queries.GetOverviewAsync(ct)))
            .WithName("GetOverview")
            .Produces<DeliveryOverviewDto>();

        group.MapGet("/work-items", async (DeliveryQueries queries, CancellationToken ct) =>
                TypedResults.Ok(await queries.GetWorkItemsAsync(ct)))
            .WithName("GetWorkItems")
            .Produces<IReadOnlyList<WorkItemSummaryDto>>();

        group.MapGet("/work-items/{id:guid}", async Task<Results<Ok<WorkItemDetailDto>, NotFound>> (
                Guid id,
                DeliveryQueries queries,
                CancellationToken ct) =>
            {
                var detail = await queries.GetWorkItemDetailAsync(id, ct);
                return detail is null ? TypedResults.NotFound() : TypedResults.Ok(detail);
            })
            .WithName("GetWorkItemDetail");

        group.MapGet("/sync-health", async (DeliveryQueries queries, CancellationToken ct) =>
                TypedResults.Ok(await queries.GetSyncHealthAsync(ct)))
            .WithName("GetSyncHealth")
            .Produces<SyncHealthDto>();

        group.MapGet("/cache-health", (DeliveryQueries queries) =>
                TypedResults.Ok(queries.GetCacheHealth()))
            .WithName("GetCacheHealth")
            .Produces<CacheHealthDto>();

        // The single state-changing endpoint. It is rate limited separately and always audited.
        group.MapPost("/work-items/{id:guid}/authorize", async Task<Results<Ok<AuthorizeResponse>, ForbidHttpResult, ProblemHttpResult>> (
                Guid id,
                [FromBody] AuthorizeRequest request,
                AuthorizeWorkItemHandler handler,
                IOptions<DeliveryOptions> options,
                CancellationToken ct) =>
            {
                var actor = request.Actor?.Trim();
                if (string.IsNullOrWhiteSpace(actor))
                {
                    return TypedResults.Problem("An actor is required.", statusCode: StatusCodes.Status400BadRequest);
                }

                var isOwner = string.Equals(actor, options.Value.RepositoryOwner, StringComparison.OrdinalIgnoreCase);

                try
                {
                    var authorized = await handler.HandleAsync(new AuthorizeWorkItemCommand(id, actor, isOwner), ct);
                    return authorized
                        ? TypedResults.Ok(new AuthorizeResponse(true, "Authorized."))
                        : TypedResults.Forbid();
                }
                catch (DomainException ex)
                {
                    return TypedResults.Problem(ex.Message, statusCode: StatusCodes.Status409Conflict);
                }
            })
            .WithName("AuthorizeWorkItem")
            .RequireRateLimiting(RateLimitPolicies.Write);

        return app;
    }
}

public sealed record AuthorizeRequest(string? Actor);

public sealed record AuthorizeResponse(bool Authorized, string Message);
