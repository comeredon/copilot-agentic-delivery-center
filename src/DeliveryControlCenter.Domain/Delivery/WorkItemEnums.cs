namespace DeliveryControlCenter.Domain.Delivery;

public enum WorkItemKind
{
    Issue = 0,
    PullRequest = 1,
}

public enum WorkItemState
{
    Open = 0,
    Closed = 1,
    Merged = 2,
    Draft = 3,
}

public enum RiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
}
