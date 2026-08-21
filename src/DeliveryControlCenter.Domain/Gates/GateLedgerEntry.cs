using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DeliveryControlCenter.Domain.Common;

namespace DeliveryControlCenter.Domain.Gates;

/// <summary>
/// One hash-chained cross-verification record. A proposing agent and an independent
/// verifying agent must both report a verdict; the orchestrator may only advance when
/// they agree. This is the concrete implementation of "agents verify each other".
/// </summary>
public sealed class GateLedgerEntry : Entity
{
    public const string GenesisHash = "sha256:0000000000000000000000000000000000000000000000000000000000000000";

    private GateLedgerEntry()
    {
        GateId = string.Empty;
        EvidenceHash = string.Empty;
        PreviousEntryHash = string.Empty;
        EntryHash = string.Empty;
    }

    public required long Sequence { get; init; }

    public required string GateId { get; init; }

    public required AgentVerdict Proposer { get; init; }

    public required AgentVerdict Verifier { get; init; }

    public string EvidenceHash { get; private set; }

    public string PreviousEntryHash { get; private set; }

    public string EntryHash { get; private set; }

    public bool Agreed => Proposer.Verdict == Verifier.Verdict;

    public static GateLedgerEntry Append(
        long sequence,
        string gateId,
        AgentVerdict proposer,
        AgentVerdict verifier,
        string evidenceHash,
        string? previousEntryHash)
    {
        DomainException.ThrowIfNullOrWhiteSpace(gateId, nameof(gateId));
        DomainException.ThrowIfNullOrWhiteSpace(evidenceHash, nameof(evidenceHash));

        if (proposer.Agent == verifier.Agent)
        {
            throw new DomainException("An agent cannot verify its own work; proposer and verifier must differ.");
        }

        var entry = new GateLedgerEntry
        {
            Sequence = sequence,
            GateId = gateId,
            Proposer = proposer,
            Verifier = verifier,
            EvidenceHash = evidenceHash,
            PreviousEntryHash = previousEntryHash ?? GenesisHash,
        };

        entry.EntryHash = entry.ComputeHash();
        return entry;
    }

    public bool VerifyIntegrity() => string.Equals(EntryHash, ComputeHash(), StringComparison.Ordinal);

    private string ComputeHash()
    {
        var payload = string.Join(
            '\n',
            Sequence.ToString(CultureInfo.InvariantCulture),
            GateId,
            Proposer.ToCanonicalString(),
            Verifier.ToCanonicalString(),
            EvidenceHash,
            PreviousEntryHash);

        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return "sha256:" + Convert.ToHexStringLower(digest);
    }
}

/// <summary>A single agent's structured verdict, including which model produced it.</summary>
public sealed record AgentVerdict(string Agent, string Model, GateVerdict Verdict)
{
    public string ToCanonicalString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Agent}|{Model}|{Verdict}");
}
