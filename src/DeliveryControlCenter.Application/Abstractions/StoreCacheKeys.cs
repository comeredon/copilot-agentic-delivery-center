using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DeliveryControlCenter.Application.Catalog;

namespace DeliveryControlCenter.Application.Abstractions;

public static class StoreCacheKeys
{
    private const string Prefix = "ninth-parallel:v1:";

    public static string Catalog(CatalogFilter filter)
    {
        var normalized = filter.Normalize();
        var payload = JsonSerializer.Serialize(normalized);
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return $"{Prefix}catalog:{Convert.ToHexStringLower(digest)}";
    }

    public static string Coffee(string slug) => $"{Prefix}coffee:{slug.ToLowerInvariant()}";

    public static string Producer(string slug) => $"{Prefix}producer:{slug.ToLowerInvariant()}";
}

public static class StoreCacheTags
{
    public const string Catalog = "ninth-parallel:v1:catalog";

    public static string Coffee(string slug) => $"ninth-parallel:v1:coffee:{slug.ToLowerInvariant()}";

    public static string Producer(string slug) => $"ninth-parallel:v1:producer:{slug.ToLowerInvariant()}";
}
