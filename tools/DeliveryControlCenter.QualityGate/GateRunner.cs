using System.Diagnostics;
using System.Globalization;

namespace DeliveryControlCenter.QualityGate;

/// <summary>
/// Runs the check set for a profile and reports pass/fail per check. Commands are a fixed,
/// hard-coded set: nothing here takes a command string from an agent or from the environment.
/// </summary>
internal sealed class GateRunner(GateOptions options)
{
    private readonly Dictionary<string, long> _durations = [];

    public IReadOnlyDictionary<string, long> Durations => _durations;

    public async Task<IReadOnlyList<GateCheck>> RunAsync(CancellationToken cancellationToken)
    {
        var checks = new List<GateCheck>
        {
            await RunAsync("restore", true, "dotnet", "restore --locked-mode", cancellationToken),
            await RunAsync("format", true, "dotnet", "format --verify-no-changes --verbosity quiet", cancellationToken),
            await RunAsync("build", true, "dotnet", "build --no-restore -warnaserror", cancellationToken),
            await RunAsync("unit-tests", true, "dotnet", "test tests/DeliveryControlCenter.UnitTests --no-build", cancellationToken),
            await RunAsync("architecture-tests", true, "dotnet", "test tests/DeliveryControlCenter.ArchitectureTests --no-build", cancellationToken),
            await RunAsync("gate-tests", true, "dotnet", "test tests/DeliveryControlCenter.QualityGate.Tests --no-build", cancellationToken),
        };

        // Integration tests need a container runtime, which the cloud agent does not have.
        // Rather than pretend, the check is recorded as not-required and not-run for that profile.
        if (options.Profile >= GateProfile.Ci)
        {
            checks.Add(await RunAsync(
                "integration-tests",
                true,
                "dotnet",
                "test tests/DeliveryControlCenter.IntegrationTests --no-build",
                cancellationToken));
        }
        else
        {
            checks.Add(new GateCheck(
                "integration-tests",
                false,
                false,
                "Skipped: the cloud profile has no container runtime. A cloud pass cannot satisfy a ci requirement."));
        }

        return [.. checks.OrderBy(c => c.Name, StringComparer.Ordinal)];
    }

    private async Task<GateCheck> RunAsync(
        string name,
        bool required,
        string fileName,
        string arguments,
        CancellationToken cancellationToken)
    {
        if (options.SkipExecution)
        {
            _durations[name] = 0;
            return new GateCheck(name, required, true, "Dry run: not executed.");
        }

        var stopwatch = Stopwatch.StartNew();

        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = options.RepositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        foreach (var argument in arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            startInfo.ArgumentList.Add(argument);
        }

        try
        {
            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException($"Could not start '{fileName}'.");

            var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            stopwatch.Stop();
            _durations[name] = stopwatch.ElapsedMilliseconds;

            var passed = process.ExitCode == 0;
            var detail = passed
                ? "Completed with exit code 0."
                : Summarize(await stderr, await stdout);

            return new GateCheck(name, required, passed, detail);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            _durations[name] = stopwatch.ElapsedMilliseconds;
            return new GateCheck(name, required, false, $"Execution failed: {ex.GetType().Name}.");
        }
    }

    /// <summary>
    /// Keeps the detail short and stable. Long, machine-specific output would break determinism,
    /// so only the first meaningful lines survive into the canonical report.
    /// </summary>
    private static string Summarize(string standardError, string standardOutput)
    {
        var source = string.IsNullOrWhiteSpace(standardError) ? standardOutput : standardError;

        var lines = source
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => l.Contains("error", StringComparison.OrdinalIgnoreCase)
                        || l.Contains("failed", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(l => l, StringComparer.Ordinal)
            .Take(5)
            .ToArray();

        return lines.Length == 0
            ? "Failed without a recognisable error line."
            : string.Join(" | ", lines).Length > 2048
                ? string.Join(" | ", lines)[..2048]
                : string.Join(" | ", lines);
    }

    public static string FormatDuration(long milliseconds) =>
        milliseconds.ToString(CultureInfo.InvariantCulture);
}
