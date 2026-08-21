using System.Text.Json.Nodes;

namespace DeliveryControlCenter.QualityGate;

internal static class ReportBuilder
{
    public const string SchemaVersion = "1.0.0";
    public const string ToolVersion = "1.0.0";

    /// <summary>
    /// Builds the report in two parts: <c>canonical</c> holds only reproducible facts and is the
    /// thing that gets hashed; <c>envelope</c> holds timestamps and durations, which differ on
    /// every run and therefore must never influence the hash.
    /// </summary>
    public static JsonObject Build(
        GateOptions options,
        IReadOnlyList<GateCheck> checks,
        IReadOnlyDictionary<string, long> durations,
        DateTimeOffset generatedAtUtc)
    {
        var verdict = DetermineVerdict(checks);

        var canonical = new JsonObject
        {
            ["commit"] = options.Commit,
            ["profile"] = options.Profile.ToString().ToLowerInvariant(),
            ["verdict"] = verdict,
            ["checks"] = new JsonArray([.. checks.Select(c => (JsonNode)new JsonObject
            {
                ["name"] = c.Name,
                ["required"] = c.Required,
                ["passed"] = c.Passed,
                ["detail"] = c.Detail,
            })]),
        };

        var canonicalJson = CanonicalJson.Serialize(canonical);
        var hash = CanonicalJson.Hash(canonicalJson);

        var durationNode = new JsonObject();
        foreach (var (name, value) in durations.OrderBy(d => d.Key, StringComparer.Ordinal))
        {
            durationNode[name] = value;
        }

        return new JsonObject
        {
            ["schemaVersion"] = SchemaVersion,
            ["canonical"] = JsonNode.Parse(canonicalJson),
            ["canonicalHash"] = hash,
            ["envelope"] = new JsonObject
            {
                ["generatedAtUtc"] = generatedAtUtc.ToString("O"),
                ["toolVersion"] = ToolVersion,
                ["durationsMs"] = durationNode,
            },
        };
    }

    private static string DetermineVerdict(IReadOnlyList<GateCheck> checks)
    {
        var required = checks.Where(c => c.Required).ToList();

        if (required.Count == 0)
        {
            return "inconclusive";
        }

        return required.All(c => c.Passed) ? "pass" : "fail";
    }
}
