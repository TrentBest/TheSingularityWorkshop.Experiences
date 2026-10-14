namespace TheSingularityWorkshop.Games.CasinoLedger;

/// <summary>A signed balance change to one account in one currency.</summary>
/// <param name="AccountId">The registered account identifier.</param>
/// <param name="Currency">A currency or token code, normalized to uppercase invariant.</param>
/// <param name="Amount">The signed change: negative debits an account; positive credits it.</param>
/// <remarks>Amounts use decimal arithmetic. A transaction must balance independently for every currency.</remarks>
public sealed record LedgerPosting
{
    /// <summary>Creates a posting.</summary>
    /// <exception cref="ArgumentException">An account or currency identifier is blank or invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The amount is zero.</exception>
    public LedgerPosting(string accountId, string currency, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID cannot be blank.", nameof(accountId));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be blank.", nameof(currency));
        if (amount == 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "A posting amount cannot be zero.");

        AccountId = accountId;
        Currency = currency.Trim().ToUpperInvariant();
        Amount = amount;
    }

    /// <summary>Gets the account identifier.</summary>
    public string AccountId { get; }

    /// <summary>Gets the normalized currency code.</summary>
    public string Currency { get; }

    /// <summary>Gets the signed balance change.</summary>
    public decimal Amount { get; }
}
