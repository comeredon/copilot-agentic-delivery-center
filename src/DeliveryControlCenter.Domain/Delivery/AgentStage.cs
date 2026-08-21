namespace DeliveryControlCenter.Domain.Delivery;

/// <summary>Where a unit of work sits in the agentic delivery flow. Ordered by progression.</summary>
public enum AgentStage
{
    Unassigned = 0,
    Scoped = 1,
    Planned = 2,
    Critiqued = 3,
    Implementing = 4,
    UnderReview = 5,
    GateFailed = 6,
    GatePassed = 7,
    AwaitingHumanAuthorization = 8,
    Merged = 9,
    NeedsHuman = 99,
}
