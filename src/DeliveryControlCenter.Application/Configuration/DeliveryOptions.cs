namespace DeliveryControlCenter.Application.Configuration;

/// <summary>Bound from the <c>Delivery</c> configuration section.</summary>
public sealed class DeliveryOptions
{
    public const string SectionName = "Delivery";

    public string RepositoryOwner { get; set; } = string.Empty;

    public string RepositoryName { get; set; } = string.Empty;

    public int? ProjectNumber { get; set; }

    /// <summary>Guarded Project field writes stay off unless an operator turns them on.</summary>
    public bool EnableProjectWrites { get; set; }

    public TimeSpan SyncInterval { get; set; } = TimeSpan.FromSeconds(60);

    public string RepositoryFullName => $"{RepositoryOwner}/{RepositoryName}";
}
