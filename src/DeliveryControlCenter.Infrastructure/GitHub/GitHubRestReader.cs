using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Domain.Delivery;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DeliveryControlCenter.Infrastructure.GitHub;

/// <summary>
/// Read-only GitHub REST adapter. Conditional requests keep the rate-limit budget intact, and
/// a 304 short-circuits the whole sync cycle. No write verb is reachable from this type.
/// </summary>
internal sealed class GitHubRestReader(
    HttpClient http,
    IGitHubTokenProvider tokenProvider,
    IOptions<GitHubOptions> options,
    ILogger<GitHubRestReader> logger) : IGitHubReader
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly GitHubOptions _options = options.Value;

    public Task<RemoteSyncResult> FetchIssuesAsync(string? etag, CancellationToken cancellationToken) =>
        FetchAsync("issues", WorkItemKind.Issue, etag, cancellationToken);

    public Task<RemoteSyncResult> FetchPullRequestsAsync(string? etag, CancellationToken cancellationToken) =>
        FetchAsync("pulls", WorkItemKind.PullRequest, etag, cancellationToken);

    private async Task<RemoteSyncResult> FetchAsync(
        string resource,
        WorkItemKind kind,
        string? etag,
        CancellationToken cancellationToken)
    {
        var path = string.Create(
            CultureInfo.InvariantCulture,
            $"repos/{_options.Owner}/{_options.Repository}/{resource}?state=all&per_page={_options.PageSize}");

        using var request = new HttpRequestMessage(HttpMethod.Get, path);

        if (!string.IsNullOrWhiteSpace(etag))
        {
            request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag));
        }

        var token = await tokenProvider.GetTokenAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await http.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            return RemoteSyncResult.Unchanged(etag);
        }

        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
        {
            // Never echo the token or full response; only the fact that we were throttled.
            logger.LogWarning("GitHub throttled the {Resource} request; keeping the previous cursor.", resource);
            return RemoteSyncResult.Unchanged(etag);
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<List<GitHubItemPayload>>(
            stream,
            SerializerOptions,
            cancellationToken) ?? [];

        var items = payload
            .Where(p => kind == WorkItemKind.PullRequest || p.PullRequest is null)
            .Select(p => new RemoteWorkItem(
                p.Number,
                kind,
                p.Title ?? $"#{p.Number}",
                p.HtmlUrl ?? string.Empty,
                MapState(p, kind)))
            .ToList();

        return new RemoteSyncResult(false, response.Headers.ETag?.Tag, items);
    }

    private static WorkItemState MapState(GitHubItemPayload payload, WorkItemKind kind)
    {
        if (kind == WorkItemKind.PullRequest)
        {
            if (payload.MergedAt is not null)
            {
                return WorkItemState.Merged;
            }

            if (payload.Draft == true)
            {
                return WorkItemState.Draft;
            }
        }

        return string.Equals(payload.State, "closed", StringComparison.OrdinalIgnoreCase)
            ? WorkItemState.Closed
            : WorkItemState.Open;
    }

    private sealed record GitHubItemPayload
    {
        public int Number { get; init; }

        public string? Title { get; init; }

        public string? State { get; init; }

        public string? HtmlUrl { get; init; }

        public bool? Draft { get; init; }

        public DateTimeOffset? MergedAt { get; init; }

        /// <summary>Present on issue payloads that are really pull requests, which we filter out.</summary>
        public object? PullRequest { get; init; }
    }
}
