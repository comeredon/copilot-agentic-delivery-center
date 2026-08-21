namespace DeliveryControlCenter.Domain.Common;

/// <summary>Base for persisted aggregates. <see cref="Version"/> is the optimistic concurrency token.</summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();

    public uint Version { get; protected set; }

    public DateTimeOffset CreatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;

    /// <summary>Advances the modification stamp and the concurrency token.</summary>
    protected void Touch()
    {
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        Version++;
    }
}
