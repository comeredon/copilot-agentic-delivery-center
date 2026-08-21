using DeliveryControlCenter.Domain.Common;
using DeliveryControlCenter.Domain.Gates;

namespace DeliveryControlCenter.UnitTests.Domain;

public sealed class GateLedgerEntryTests
{
    private static readonly AgentVerdict Proposer = new("dotnet-implementer", "gpt-5", GateVerdict.Passed);
    private static readonly AgentVerdict Verifier = new("quality-verifier", "claude-sonnet-4.5", GateVerdict.Passed);

    [Fact]
    public void Append_RefusesSelfVerification()
    {
        var same = new AgentVerdict("dotnet-implementer", "gpt-5", GateVerdict.Passed);

        var ex = Should.Throw<DomainException>(() =>
            GateLedgerEntry.Append(1, "gate-1", Proposer, same, "sha256:abc", null));

        ex.Message.ShouldContain("cannot verify its own work");
    }

    [Fact]
    public void Append_StartsFromGenesis()
    {
        var entry = GateLedgerEntry.Append(1, "gate-1", Proposer, Verifier, "sha256:abc", null);

        entry.PreviousEntryHash.ShouldBe(GateLedgerEntry.GenesisHash);
        entry.VerifyIntegrity().ShouldBeTrue();
    }

    [Fact]
    public void Append_ChainsToThePreviousEntry()
    {
        var first = GateLedgerEntry.Append(1, "gate-1", Proposer, Verifier, "sha256:abc", null);
        var second = GateLedgerEntry.Append(2, "gate-2", Proposer, Verifier, "sha256:def", first.EntryHash);

        second.PreviousEntryHash.ShouldBe(first.EntryHash);
        second.EntryHash.ShouldNotBe(first.EntryHash);
        second.VerifyIntegrity().ShouldBeTrue();
    }

    [Fact]
    public void Append_IsDeterministicForIdenticalInput()
    {
        var a = GateLedgerEntry.Append(7, "gate-7", Proposer, Verifier, "sha256:abc", GateLedgerEntry.GenesisHash);
        var b = GateLedgerEntry.Append(7, "gate-7", Proposer, Verifier, "sha256:abc", GateLedgerEntry.GenesisHash);

        a.EntryHash.ShouldBe(b.EntryHash);
    }

    [Fact]
    public void Agreed_IsFalseWhenTheVerifierDisagrees()
    {
        var dissenting = new AgentVerdict("quality-verifier", "claude-sonnet-4.5", GateVerdict.Failed);

        var entry = GateLedgerEntry.Append(1, "gate-1", Proposer, dissenting, "sha256:abc", null);

        entry.Agreed.ShouldBeFalse();
    }
}
