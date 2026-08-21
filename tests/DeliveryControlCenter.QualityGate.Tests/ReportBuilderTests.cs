using DeliveryControlCenter.QualityGate;

namespace DeliveryControlCenter.QualityGateTests;

public sealed class ReportBuilderTests
{
    private static readonly GateOptions Options = new(
        GateProfile.Ci,
        "abc1234",
        ".",
        "artifacts/quality-report.json",
        SkipExecution: true);

    [Fact]
    public void CanonicalHash_IgnoresTimestampsAndDurations()
    {
        // This is the determinism contract: the same verdict on the same commit must produce the
        // same hash regardless of when it ran or how long it took.
        var checks = new[] { new GateCheck("build", true, true, "ok") };

        var first = ReportBuilder.Build(Options, checks, new Dictionary<string, long> { ["build"] = 10 }, DateTimeOffset.UnixEpoch);
        var second = ReportBuilder.Build(Options, checks, new Dictionary<string, long> { ["build"] = 9_999 }, DateTimeOffset.UtcNow);

        first["canonicalHash"]!.GetValue<string>()
            .ShouldBe(second["canonicalHash"]!.GetValue<string>());
    }

    [Fact]
    public void CanonicalHash_ChangesWhenACheckResultChanges()
    {
        var pass = ReportBuilder.Build(Options, [new GateCheck("build", true, true, "ok")], Empty, DateTimeOffset.UnixEpoch);
        var fail = ReportBuilder.Build(Options, [new GateCheck("build", true, true, "boom")], Empty, DateTimeOffset.UnixEpoch);

        pass["canonicalHash"]!.GetValue<string>()
            .ShouldNotBe(fail["canonicalHash"]!.GetValue<string>());
    }

    [Fact]
    public void Verdict_IsFailWhenARequiredCheckFailed()
    {
        var report = ReportBuilder.Build(
            Options,
            [new GateCheck("build", true, true, "ok"), new GateCheck("unit-tests", true, false, "2 failing")],
            Empty,
            DateTimeOffset.UnixEpoch);

        report["canonical"]!["verdict"]!.GetValue<string>().ShouldBe("fail");
    }

    [Fact]
    public void Verdict_IgnoresOptionalChecks()
    {
        var report = ReportBuilder.Build(
            Options,
            [new GateCheck("build", true, true, "ok"), new GateCheck("integration-tests", false, false, "skipped")],
            Empty,
            DateTimeOffset.UnixEpoch);

        report["canonical"]!["verdict"]!.GetValue<string>().ShouldBe("pass");
    }

    [Fact]
    public void Report_ValidatesAgainstTheCommittedSchema()
    {
        var report = ReportBuilder.Build(Options, [new GateCheck("build", true, true, "ok")], Empty, DateTimeOffset.UnixEpoch);
        var schemaPath = Path.Combine(RepositoryRoot(), "schemas", "quality-report.schema.json");

        var errors = new ReportValidator(schemaPath).Validate(report);

        errors.ShouldBeEmpty();
    }

    [Fact]
    public void Validator_RejectsAMalformedReport()
    {
        var report = ReportBuilder.Build(Options, [new GateCheck("build", true, true, "ok")], Empty, DateTimeOffset.UnixEpoch);
        report["canonical"]!["verdict"] = "definitely-fine";
        var schemaPath = Path.Combine(RepositoryRoot(), "schemas", "quality-report.schema.json");

        var errors = new ReportValidator(schemaPath).Validate(report);

        errors.ShouldNotBeEmpty();
    }

    private static Dictionary<string, long> Empty => [];

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
