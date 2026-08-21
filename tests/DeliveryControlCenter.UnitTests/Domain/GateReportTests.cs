using DeliveryControlCenter.Domain.Gates;

namespace DeliveryControlCenter.UnitTests.Domain;

public sealed class GateReportTests
{
    [Fact]
    public void Checks_AreSortedOnIngestSoTheOrderIsStable()
    {
        var report = NewReport(checks:
        [
            new GateCheckResult("unit-tests", true, "ok"),
            new GateCheckResult("build", true, "ok"),
            new GateCheckResult("format", true, "ok"),
        ]);

        report.Checks.Select(c => c.Name).ShouldBe(["build", "format", "unit-tests"]);
    }

    [Theory]
    [InlineData(QualityGateProfile.Local, QualityGateProfile.Ci, true)]
    [InlineData(QualityGateProfile.Ci, QualityGateProfile.Ci, true)]
    [InlineData(QualityGateProfile.Cloud, QualityGateProfile.Ci, false)]
    public void SatisfiesRequirement_ComparesProfileStrength(
        QualityGateProfile actual,
        QualityGateProfile required,
        bool expected)
    {
        var report = NewReport(profile: actual);

        report.SatisfiesRequirement(required).ShouldBe(expected);
    }

    [Fact]
    public void Verdict_IsFailedWhenAnyCheckFailed()
    {
        var report = NewReport(checks:
        [
            new GateCheckResult("build", true, "ok"),
            new GateCheckResult("unit-tests", false, "2 failing"),
        ]);

        report.Verdict.ShouldBe(GateVerdict.Failed);
        report.SatisfiesRequirement(QualityGateProfile.Cloud).ShouldBeFalse();
    }

    [Fact]
    public void Verdict_IsInconclusiveWhenNothingRan()
    {
        var report = NewReport(checks: []);

        report.Verdict.ShouldBe(GateVerdict.Inconclusive);
    }

    private static GateReport NewReport(
        QualityGateProfile profile = QualityGateProfile.Ci,
        GateCheckResult[]? checks = null) =>
        GateReport.Create(
            "abc1234",
            profile,
            "1.0.0",
            "sha256:" + new string('a', 64),
            checks ?? [new GateCheckResult("build", true, "ok")],
            Guid.CreateVersion7());
}
