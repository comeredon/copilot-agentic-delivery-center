using DeliveryControlCenter.QualityGate;

var options = GateOptions.Parse(args);

Console.WriteLine($"Quality gate: profile={options.Profile.ToString().ToLowerInvariant()} commit={options.Commit}");

var runner = new GateRunner(options);
var checks = await runner.RunAsync(CancellationToken.None);

var report = ReportBuilder.Build(options, checks, runner.Durations, DateTimeOffset.UtcNow);

var schemaPath = Path.Combine(options.RepositoryRoot, "schemas", "quality-report.schema.json");
var validationErrors = new ReportValidator(schemaPath).Validate(report);

Directory.CreateDirectory(Path.GetDirectoryName(options.OutputPath)!);
await File.WriteAllTextAsync(options.OutputPath, report.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

foreach (var check in checks)
{
    var status = check.Passed ? "pass" : check.Required ? "FAIL" : "skip";
    Console.WriteLine($"  [{status}] {check.Name} - {check.Detail}");
}

Console.WriteLine($"Canonical hash: {report["canonicalHash"]}");
Console.WriteLine($"Report written to: {options.OutputPath}");

if (validationErrors.Count > 0)
{
    Console.Error.WriteLine("Report failed schema validation:");
    foreach (var error in validationErrors)
    {
        Console.Error.WriteLine($"  {error}");
    }

    return 2;
}

var verdict = report["canonical"]?["verdict"]?.GetValue<string>();
Console.WriteLine($"Verdict: {verdict}");

return verdict == "pass" ? 0 : 1;
