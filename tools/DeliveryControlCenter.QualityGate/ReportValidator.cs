using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using Json.Schema;

namespace DeliveryControlCenter.QualityGate;

/// <summary>
/// Validates the report against the committed schema before it is written. A report that does
/// not validate is treated as a gate failure, so a malformed artifact can never look like a pass.
/// </summary>
internal sealed class ReportValidator(string schemaPath)
{
    // The schema registry is process-wide and rejects re-registering the same $id, so each
    // schema file is loaded exactly once per process.
    private static readonly ConcurrentDictionary<string, JsonSchema> SchemaCache = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> Validate(JsonObject report)
    {
        if (!File.Exists(schemaPath))
        {
            return [$"Schema not found at '{schemaPath}'."];
        }

        var schema = SchemaCache.GetOrAdd(Path.GetFullPath(schemaPath), path => JsonSchema.FromFile(path));
        using var document = System.Text.Json.JsonDocument.Parse(report.ToJsonString());
        var result = schema.Evaluate(document.RootElement, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
            RequireFormatValidation = true,
        });

        if (result.IsValid)
        {
            return [];
        }

        return [.. Flatten(result)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(d => d, StringComparer.Ordinal)];
    }

    private static IEnumerable<string> Flatten(EvaluationResults results)
    {
        if (results.Errors is not null)
        {
            foreach (var error in results.Errors)
            {
                yield return $"{results.InstanceLocation}: {error.Value}";
            }
        }

        foreach (var detail in results.Details ?? [])
        {
            foreach (var message in Flatten(detail))
            {
                yield return message;
            }
        }
    }
}
