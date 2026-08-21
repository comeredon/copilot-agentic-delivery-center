namespace DeliveryControlCenter.Domain.Gates;

public enum GateVerdict
{
    Pending = 0,
    Passed = 1,
    Failed = 2,
    Inconclusive = 3,
}

/// <summary>
/// Which checks a gate run executed. A <see cref="Cloud"/> report can never satisfy a
/// required check that demands <see cref="Ci"/>, because it skips browser-based suites.
/// </summary>
public enum QualityGateProfile
{
    Cloud = 0,
    Ci = 1,
    Local = 2,
}
