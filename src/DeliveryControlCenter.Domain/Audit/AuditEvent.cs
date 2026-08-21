using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Audit;

/// <summary>Append-only record of every state-changing action, human or agent.</summary>
public sealed class AuditEvent : Entity
{
    private AuditEvent()
    {
        Actor = string.Empty;
        Action = string.Empty;
        Target = string.Empty;
        Outcome = string.Empty;
    }

    public required string Actor { get; init; }

    public required string Action { get; init; }

    public required string Target { get; init; }

    public required string Outcome { get; init; }

    public bool Denied { get; private set; }

    public static AuditEvent Allowed(string actor, string action, string target, string outcome) =>
        Create(actor, action, target, outcome, denied: false);

    /// <summary>Records a blocked attempt, such as an agent trying to merge.</summary>
    public static AuditEvent Blocked(string actor, string action, string target, string reason) =>
        Create(actor, action, target, reason, denied: true);

    private static AuditEvent Create(string actor, string action, string target, string outcome, bool denied)
    {
        DomainException.ThrowIfNullOrWhiteSpace(actor, nameof(actor));
        DomainException.ThrowIfNullOrWhiteSpace(action, nameof(action));

        return new AuditEvent
        {
            Actor = actor,
            Action = action,
            Target = target ?? string.Empty,
            Outcome = outcome ?? string.Empty,
            Denied = denied,
        };
    }
}
