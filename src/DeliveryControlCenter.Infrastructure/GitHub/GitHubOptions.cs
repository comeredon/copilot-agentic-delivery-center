namespace DeliveryControlCenter.Infrastructure.GitHub;

public sealed class GitHubOptions
{
    public const string SectionName = "GitHub";

    /// <summary>Fixed API root. Agents must never redirect this at an arbitrary host.</summary>
    public Uri BaseAddress { get; set; } = new("https://api.github.com/");

    public string Owner { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;

    /// <summary>When false the app runs from seeded data, so a clean clone works offline.</summary>
    public bool Enabled { get; set; }

    /// <summary>Guarded Project field writes. Off by default.</summary>
    public bool EnableProjectWrites { get; set; }

    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Supplies the bearer token for read-only calls. The installation-token implementation for the
/// GitHub App is delivered separately; a static token keeps local development working.
/// </summary>
public interface IGitHubTokenProvider
{
    ValueTask<string?> GetTokenAsync(CancellationToken cancellationToken);
}

internal sealed class StaticGitHubTokenProvider(string? token) : IGitHubTokenProvider
{
    public ValueTask<string?> GetTokenAsync(CancellationToken cancellationToken) => ValueTask.FromResult(token);
}
