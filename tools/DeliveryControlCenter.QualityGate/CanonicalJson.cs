using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DeliveryControlCenter.QualityGate;

/// <summary>
/// Canonical JSON serialisation: object keys sorted ordinal, no insignificant whitespace, and
/// UTF-8 without a BOM. Two runs that reach the same conclusion produce byte-identical output,
/// which is what makes the hash meaningful as a verdict identity.
/// </summary>
internal static class CanonicalJson
{
    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Indented = false,
        SkipValidation = false,
    };

    public static string Serialize(JsonNode node)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
        {
            Write(node, writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static string Hash(string canonicalJson)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalJson));
        return $"sha256:{Convert.ToHexStringLower(bytes)}";
    }

    private static void Write(JsonNode? node, Utf8JsonWriter writer)
    {
        switch (node)
        {
            case null:
                writer.WriteNullValue();
                break;

            case JsonObject obj:
                writer.WriteStartObject();
                foreach (var property in obj.OrderBy(p => p.Key, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Key);
                    Write(property.Value, writer);
                }

                writer.WriteEndObject();
                break;

            case JsonArray array:
                writer.WriteStartArray();
                foreach (var item in array)
                {
                    Write(item, writer);
                }

                writer.WriteEndArray();
                break;

            default:
                node.WriteTo(writer);
                break;
        }
    }
}
