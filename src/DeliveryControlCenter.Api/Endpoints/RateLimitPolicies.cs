using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace DeliveryControlCenter.Api.Endpoints;

internal static class RateLimitPolicies
{
    public const string Read = "read";
    public const string Write = "write";

    /// <summary>
    /// Partitioned by remote address so one noisy client cannot exhaust the budget for everyone.
    /// The write policy is deliberately far tighter than the read policy.
    /// </summary>
    public static void AddDeliveryRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(Read, context => RateLimitPartition.GetFixedWindowLimiter(
                PartitionKey(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 240,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));

            options.AddPolicy(Write, context => RateLimitPartition.GetFixedWindowLimiter(
                PartitionKey(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));
        });

    private static string PartitionKey(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    public static string Describe(int permitLimit, TimeSpan window) =>
        string.Create(CultureInfo.InvariantCulture, $"{permitLimit} requests per {window.TotalSeconds:N0}s");
}
