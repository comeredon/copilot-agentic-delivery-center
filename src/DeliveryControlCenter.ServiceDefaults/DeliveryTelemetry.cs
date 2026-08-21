using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Microsoft.Extensions.Hosting;

/// <summary>Named instruments so dashboards and tests reference the same strings.</summary>
public static class DeliveryTelemetry
{
    public const string ActivitySourceName = "DeliveryControlCenter";
    public const string MeterName = "DeliveryControlCenter";

    public static ActivitySource ActivitySource { get; } = new(ActivitySourceName);

    public static Meter Meter { get; } = new(MeterName);
}
