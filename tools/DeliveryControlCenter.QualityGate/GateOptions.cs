namespace DeliveryControlCenter.QualityGate;

/// <summary>
/// Profiles exist because the cloud agent has no container runtime. A cloud run that skips
/// integration tests must not be able to claim the same verdict strength as a CI run, so the
/// profile travels with the report and the merge policy compares against it.
/// </summary>
internal enum GateProfile
{
    Cloud = 0,
    Ci = 1,
    Local = 2,
}

internal sealed record GateCheck(string Name, bool Required, bool Passed, string Detail);

internal sealed record GateOptions(
    GateProfile Profile,
    string Commit,
    string RepositoryRoot,
    string OutputPath,
    bool SkipExecution)
{
    public static GateOptions Parse(string[] args)
    {
        var profile = GateProfile.Local;
        var commit = "unknown";
        var root = Directory.GetCurrentDirectory();
        var output = Path.Combine("artifacts", "quality-report.json");
        var skip = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--profile" when i + 1 < args.Length:
                    profile = Enum.Parse<GateProfile>(args[++i], ignoreCase: true);
                    break;
                case "--commit" when i + 1 < args.Length:
                    commit = args[++i].ToLowerInvariant();
                    break;
                case "--root" when i + 1 < args.Length:
                    root = Path.GetFullPath(args[++i]);
                    break;
                case "--output" when i + 1 < args.Length:
                    output = args[++i];
                    break;
                case "--dry-run":
                    skip = true;
                    break;
                default:
                    break;
            }
        }

        return new GateOptions(profile, commit, root, Path.GetFullPath(output, root), skip);
    }
}
