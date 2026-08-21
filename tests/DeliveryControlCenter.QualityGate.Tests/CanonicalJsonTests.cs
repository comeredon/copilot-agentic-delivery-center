using System.Text.Json.Nodes;
using DeliveryControlCenter.QualityGate;

namespace DeliveryControlCenter.QualityGateTests;

public sealed class CanonicalJsonTests
{
    [Fact]
    public void Serialize_SortsObjectKeysOrdinal()
    {
        var node = new JsonObject
        {
            ["zulu"] = 1,
            ["alpha"] = 2,
            ["Mike"] = 3,
        };

        CanonicalJson.Serialize(node).ShouldBe("""{"Mike":3,"alpha":2,"zulu":1}""");
    }

    [Fact]
    public void Serialize_PreservesArrayOrder()
    {
        // Arrays are ordered data; reordering them would change meaning, not just formatting.
        var node = new JsonObject { ["items"] = new JsonArray(3, 1, 2) };

        CanonicalJson.Serialize(node).ShouldBe("""{"items":[3,1,2]}""");
    }

    [Fact]
    public void Serialize_IsIndependentOfInsertionOrder()
    {
        var first = new JsonObject { ["a"] = 1, ["b"] = new JsonObject { ["y"] = 2, ["x"] = 3 } };
        var second = new JsonObject { ["b"] = new JsonObject { ["x"] = 3, ["y"] = 2 }, ["a"] = 1 };

        CanonicalJson.Serialize(first).ShouldBe(CanonicalJson.Serialize(second));
    }

    [Fact]
    public void Hash_IsStableAndPrefixed()
    {
        var json = CanonicalJson.Serialize(new JsonObject { ["a"] = 1 });

        var hash = CanonicalJson.Hash(json);

        hash.ShouldStartWith("sha256:");
        hash.Length.ShouldBe("sha256:".Length + 64);
        CanonicalJson.Hash(json).ShouldBe(hash);
    }

    [Fact]
    public void Hash_ChangesWhenContentChanges()
    {
        var a = CanonicalJson.Hash(CanonicalJson.Serialize(new JsonObject { ["a"] = 1 }));
        var b = CanonicalJson.Hash(CanonicalJson.Serialize(new JsonObject { ["a"] = 2 }));

        a.ShouldNotBe(b);
    }
}
