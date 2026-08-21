using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Sync;

/// <summary>The single repository this instance observes. Reads are always read-only.</summary>
public sealed class RepositoryConnection : Entity
{
    private RepositoryConnection()
    {
        Owner = string.Empty;
        Name = string.Empty;
    }

    public required string Owner { get; init; }

    public required string Name { get; init; }

    public int? ProjectNumber { get; private set; }

    /// <summary>Project field writes are opt-in and default to disabled.</summary>
    public bool ProjectWritesEnabled { get; private set; }

    public string FullName => $"{Owner}/{Name}";

    public static RepositoryConnection Create(string owner, string name, int? projectNumber = null)
    {
        DomainException.ThrowIfNullOrWhiteSpace(owner, nameof(owner));
        DomainException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return new RepositoryConnection
        {
            Owner = owner,
            Name = name,
            ProjectNumber = projectNumber,
        };
    }

    public void EnableProjectWrites(bool enabled)
    {
        if (enabled && ProjectNumber is null)
        {
            throw new DomainException("Project writes require a configured project number.");
        }

        ProjectWritesEnabled = enabled;
        Touch();
    }
}
