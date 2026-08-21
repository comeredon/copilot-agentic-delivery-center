using DeliveryControlCenter.Application.Abstractions;
using DeliveryControlCenter.Domain.Delivery;

namespace DeliveryControlCenter.Infrastructure.GitHub;

/// <summary>
/// Deterministic offline substitute used when <see cref="GitHubOptions.Enabled"/> is false, so a
/// fresh clone runs with no credentials. The payload is fixed, which keeps demo runs repeatable.
/// </summary>
internal sealed class SeededGitHubReader : IGitHubReader
{
    private const string SeedETag = "\"seed-v1\"";

    private static readonly RemoteWorkItem[] Seed =
    [
        new(101, WorkItemKind.Issue, "Add GitHub App installation token provider", "https://github.com/comeredon/copilot-agentic-delivery-center/issues/101", WorkItemState.Open),
        new(102, WorkItemKind.Issue, "Implement Projects v2 GraphQL reader", "https://github.com/comeredon/copilot-agentic-delivery-center/issues/102", WorkItemState.Open),
        new(103, WorkItemKind.Issue, "Surface cache invalidation events in the UI", "https://github.com/comeredon/copilot-agentic-delivery-center/issues/103", WorkItemState.Closed),
        new(201, WorkItemKind.PullRequest, "feat: durable sync cursors with ETag support", "https://github.com/comeredon/copilot-agentic-delivery-center/pull/201", WorkItemState.Open),
        new(202, WorkItemKind.PullRequest, "feat: gate ledger hash chain", "https://github.com/comeredon/copilot-agentic-delivery-center/pull/202", WorkItemState.Draft),
    ];

    public Task<RemoteSyncResult> FetchIssuesAsync(string? etag, CancellationToken cancellationToken) =>
        Task.FromResult(Build(etag, WorkItemKind.Issue));

    public Task<RemoteSyncResult> FetchPullRequestsAsync(string? etag, CancellationToken cancellationToken) =>
        Task.FromResult(Build(etag, WorkItemKind.PullRequest));

    private static RemoteSyncResult Build(string? etag, WorkItemKind kind)
    {
        if (string.Equals(etag, SeedETag, StringComparison.Ordinal))
        {
            return RemoteSyncResult.Unchanged(etag);
        }

        return new RemoteSyncResult(false, SeedETag, [.. Seed.Where(i => i.Kind == kind)]);
    }
}
