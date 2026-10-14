using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.Games.CasinoLedger;

/// <summary>An immutable, caller-identified batch of balanced account postings.</summary>
/// <remarks>
/// The caller supplies the transaction ID and timestamp so simulations can be replayed deterministically.
/// A transaction ID is an idempotency key: an identical replay is ignored; reusing it for different content is rejected.
/// </remarks>
public sealed class LedgerTransaction
{
    private readonly ReadOnlyCollection<LedgerPosting> _postings;

    /// <summary>Creates an immutable ledger transaction.</summary>
    /// <exception cref="ArgumentException">An identifier, kind, or posting collection is invalid.</exception>
    /// <exception cref="ArgumentNullException">The postings collection or one of its entries is null.</exception>
    public LedgerTransaction(
        string transactionId,
        string kind,
        DateTimeOffset occurredAt,
        IEnumerable<LedgerPosting> postings)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be blank.", nameof(transactionId));
        if (string.IsNullOrWhiteSpace(kind))
            throw new ArgumentException("Transaction kind cannot be blank.", nameof(kind));
        ArgumentNullException.ThrowIfNull(postings);

        var copy = postings.ToArray();
        if (copy.Length < 2)
            throw new ArgumentException("A balanced transaction needs at least two postings.", nameof(postings));
        if (copy.Any(posting => posting is null))
            throw new ArgumentException("A posting cannot be null.", nameof(postings));
        if (occurredAt == default)
            throw new ArgumentException("A transaction timestamp must be supplied.", nameof(occurredAt));

        TransactionId = transactionId;
        Kind = kind;
        OccurredAt = occurredAt;
        _postings = Array.AsReadOnly(copy);
    }

    /// <summary>Gets the idempotency key for this transaction.</summary>
    public string TransactionId { get; }

    /// <summary>Gets the caller-defined transaction kind.</summary>
    public string Kind { get; }

    /// <summary>Gets the caller-supplied event timestamp.</summary>
    public DateTimeOffset OccurredAt { get; }

    /// <summary>Gets the immutable posting snapshot.</summary>
    public IReadOnlyList<LedgerPosting> Postings => _postings;
}
