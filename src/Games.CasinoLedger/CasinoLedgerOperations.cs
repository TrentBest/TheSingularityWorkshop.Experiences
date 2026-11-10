namespace TheSingularityWorkshop.Games.CasinoLedger;

/// <summary>Builds and posts common simulation-casino transactions against a configured ledger.</summary>
/// <remarks>
/// These helpers define a simple accounting shape, not authorization, exchange-rate, fee, tax, or game policy.
/// Cash and chip amounts are supplied separately. Gross game payout includes the returned stake when the game rules
/// define it that way. All account funding and house-reserve availability are enforced by <see cref="CasinoLedger"/>.
/// </remarks>
public static class CasinoLedgerOperations
{
    /// <summary>Exchanges a caller-specified cash amount for a caller-specified amount of virtual chips.</summary>
    /// <param name="ledger">The ledger to receive the transaction.</param>
    /// <param name="transactionId">A stable idempotency key supplied by the caller.</param>
    /// <param name="occurredAt">The event timestamp supplied by the caller.</param>
    /// <param name="playerCashAccount">The account debited for cash.</param>
    /// <param name="cageCashAccount">The account credited for cash.</param>
    /// <param name="cageChipInventoryAccount">The account debited for chips.</param>
    /// <param name="playerChipAccount">The account credited for chips.</param>
    /// <param name="cashAmount">The positive cash amount.</param>
    /// <param name="chipAmount">The positive chip amount; no exchange rate is inferred.</param>
    /// <param name="cashCurrency">The cash currency code.</param>
    /// <param name="chipCurrency">The distinct chip currency code.</param>
    /// <exception cref="ArgumentNullException">The ledger is null.</exception>
    /// <exception cref="ArgumentException">A currency or account identifier is invalid, or paired accounts/currencies are not distinct.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either amount is not positive.</exception>
    /// <exception cref="InvalidOperationException">The ledger rejects the transaction, for example because an account would be overdrawn.</exception>
    /// <returns><see langword="true"/> if posted; <see langword="false"/> if the identical transaction was already posted.</returns>
    public static bool BuyIn(
        CasinoLedger ledger,
        string transactionId,
        DateTimeOffset occurredAt,
        string playerCashAccount,
        string cageCashAccount,
        string cageChipInventoryAccount,
        string playerChipAccount,
        decimal cashAmount,
        decimal chipAmount,
        string cashCurrency = "S$",
        string chipCurrency = "CHIP")
    {
        ArgumentNullException.ThrowIfNull(ledger);
        RequirePositive(cashAmount, nameof(cashAmount));
        RequirePositive(chipAmount, nameof(chipAmount));
        var normalizedCashCurrency = NormalizeCurrency(cashCurrency, nameof(cashCurrency));
        var normalizedChipCurrency = NormalizeCurrency(chipCurrency, nameof(chipCurrency));
        RequireDifferentCurrencies(normalizedCashCurrency, normalizedChipCurrency, nameof(chipCurrency));
        RequireDifferentAccounts(playerCashAccount, cageCashAccount, nameof(playerCashAccount), nameof(cageCashAccount));
        RequireDifferentAccounts(cageChipInventoryAccount, playerChipAccount, nameof(cageChipInventoryAccount), nameof(playerChipAccount));

        return ledger.Post(new LedgerTransaction(transactionId, "buy-in", occurredAt,
        [
            new LedgerPosting(playerCashAccount, normalizedCashCurrency, -cashAmount),
            new LedgerPosting(cageCashAccount, normalizedCashCurrency, cashAmount),
            new LedgerPosting(cageChipInventoryAccount, normalizedChipCurrency, -chipAmount),
            new LedgerPosting(playerChipAccount, normalizedChipCurrency, chipAmount)
        ]));
    }

    /// <summary>Exchanges a caller-specified amount of virtual chips back into a caller-specified cash amount.</summary>
    /// <param name="ledger">The ledger to receive the transaction.</param>
    /// <param name="transactionId">A stable idempotency key supplied by the caller.</param>
    /// <param name="occurredAt">The event timestamp supplied by the caller.</param>
    /// <param name="playerCashAccount">The account credited for cash.</param>
    /// <param name="cageCashAccount">The account debited for cash.</param>
    /// <param name="cageChipInventoryAccount">The account credited for chips.</param>
    /// <param name="playerChipAccount">The account debited for chips.</param>
    /// <param name="cashAmount">The positive cash amount.</param>
    /// <param name="chipAmount">The positive chip amount; no exchange rate is inferred.</param>
    /// <param name="cashCurrency">The cash currency code.</param>
    /// <param name="chipCurrency">The distinct chip currency code.</param>
    /// <exception cref="ArgumentNullException">The ledger is null.</exception>
    /// <exception cref="ArgumentException">A currency or account identifier is invalid, or paired accounts/currencies are not distinct.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Either amount is not positive.</exception>
    /// <exception cref="InvalidOperationException">The ledger rejects the transaction, for example because an account would be overdrawn.</exception>
    /// <returns><see langword="true"/> if posted; <see langword="false"/> if the identical transaction was already posted.</returns>
    public static bool CashOut(
        CasinoLedger ledger,
        string transactionId,
        DateTimeOffset occurredAt,
        string playerCashAccount,
        string cageCashAccount,
        string cageChipInventoryAccount,
        string playerChipAccount,
        decimal cashAmount,
        decimal chipAmount,
        string cashCurrency = "S$",
        string chipCurrency = "CHIP")
    {
        ArgumentNullException.ThrowIfNull(ledger);
        RequirePositive(cashAmount, nameof(cashAmount));
        RequirePositive(chipAmount, nameof(chipAmount));
        var normalizedCashCurrency = NormalizeCurrency(cashCurrency, nameof(cashCurrency));
        var normalizedChipCurrency = NormalizeCurrency(chipCurrency, nameof(chipCurrency));
        RequireDifferentCurrencies(normalizedCashCurrency, normalizedChipCurrency, nameof(chipCurrency));
        RequireDifferentAccounts(playerCashAccount, cageCashAccount, nameof(playerCashAccount), nameof(cageCashAccount));
        RequireDifferentAccounts(cageChipInventoryAccount, playerChipAccount, nameof(cageChipInventoryAccount), nameof(playerChipAccount));

        return ledger.Post(new LedgerTransaction(transactionId, "cash-out", occurredAt,
        [
            new LedgerPosting(playerChipAccount, normalizedChipCurrency, -chipAmount),
            new LedgerPosting(cageChipInventoryAccount, normalizedChipCurrency, chipAmount),
            new LedgerPosting(cageCashAccount, normalizedCashCurrency, -cashAmount),
            new LedgerPosting(playerCashAccount, normalizedCashCurrency, cashAmount)
        ]));
    }

    /// <summary>Moves virtual chips from a player's account into a table's wager escrow.</summary>
    /// <param name="ledger">The ledger to receive the transaction.</param>
    /// <param name="transactionId">A stable idempotency key supplied by the caller.</param>
    /// <param name="occurredAt">The event timestamp supplied by the caller.</param>
    /// <param name="playerChipAccount">The account debited for the wager.</param>
    /// <param name="tableEscrowAccount">The account credited with the wager.</param>
    /// <param name="wagerAmount">The positive amount to escrow.</param>
    /// <param name="chipCurrency">The chip currency code.</param>
    /// <exception cref="ArgumentNullException">The ledger is null.</exception>
    /// <exception cref="ArgumentException">An account or currency identifier is invalid, or the accounts are identical.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The wager amount is not positive.</exception>
    /// <exception cref="InvalidOperationException">The ledger rejects the transaction, for example because the player lacks funds.</exception>
    /// <returns><see langword="true"/> if posted; <see langword="false"/> if the identical transaction was already posted.</returns>
    public static bool PlaceWager(
        CasinoLedger ledger,
        string transactionId,
        DateTimeOffset occurredAt,
        string playerChipAccount,
        string tableEscrowAccount,
        decimal wagerAmount,
        string chipCurrency = "CHIP")
    {
        ArgumentNullException.ThrowIfNull(ledger);
        RequirePositive(wagerAmount, nameof(wagerAmount));
        var currency = NormalizeCurrency(chipCurrency, nameof(chipCurrency));
        RequireDifferentAccounts(playerChipAccount, tableEscrowAccount, nameof(playerChipAccount), nameof(tableEscrowAccount));

        return ledger.Post(new LedgerTransaction(transactionId, "wager", occurredAt,
        [
            new LedgerPosting(playerChipAccount, currency, -wagerAmount),
            new LedgerPosting(tableEscrowAccount, currency, wagerAmount)
        ]));
    }

    /// <summary>
    /// Settles an existing escrowed wager. A payout includes any returned stake defined by the game rules.
    /// </summary>
    /// <param name="ledger">The ledger to receive the transaction.</param>
    /// <param name="transactionId">A stable idempotency key supplied by the caller.</param>
    /// <param name="occurredAt">The event timestamp supplied by the caller.</param>
    /// <param name="tableEscrowAccount">The account debited for the full wager.</param>
    /// <param name="playerChipAccount">The account credited with the gross payout.</param>
    /// <param name="houseReserveAccount">The account debited only when payout exceeds the wager.</param>
    /// <param name="houseGamingRevenueAccount">The account credited only when payout is below the wager.</param>
    /// <param name="wagerAmount">The positive wager previously moved into escrow.</param>
    /// <param name="grossPayout">The nonnegative payout amount, including returned stake when the game rules define it that way.</param>
    /// <param name="chipCurrency">The chip currency code.</param>
    /// <exception cref="ArgumentNullException">The ledger is null.</exception>
    /// <exception cref="ArgumentException">An account or currency identifier is invalid, or required accounts are not distinct.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The wager amount is not positive or the gross payout is negative.</exception>
    /// <exception cref="InvalidOperationException">The ledger rejects the transaction, for example because escrow or house reserve lacks funds.</exception>
    /// <remarks>
    /// The table escrow is debited by the full wager. If gross payout is below the wager, the difference is credited
    /// to house gaming revenue. If gross payout exceeds the wager, the excess is debited from house reserve.
    /// The ledger rejects settlement if escrow or reserve funds are insufficient. The caller remains responsible
    /// for proving that this wager was authorized and has not already been settled under a different transaction ID.
    /// </remarks>
    /// <returns><see langword="true"/> if posted; <see langword="false"/> if the identical transaction was already posted.</returns>
    public static bool SettleWager(
        CasinoLedger ledger,
        string transactionId,
        DateTimeOffset occurredAt,
        string tableEscrowAccount,
        string playerChipAccount,
        string houseReserveAccount,
        string houseGamingRevenueAccount,
        decimal wagerAmount,
        decimal grossPayout,
        string chipCurrency = "CHIP")
    {
        ArgumentNullException.ThrowIfNull(ledger);
        RequirePositive(wagerAmount, nameof(wagerAmount));
        if (grossPayout < 0m)
            throw new ArgumentOutOfRangeException(nameof(grossPayout), "Gross payout cannot be negative.");

        var currency = NormalizeCurrency(chipCurrency, nameof(chipCurrency));
        RequireDifferentAccounts(tableEscrowAccount, playerChipAccount, nameof(tableEscrowAccount), nameof(playerChipAccount));

        var postings = new List<LedgerPosting>
        {
            new(tableEscrowAccount, currency, -wagerAmount)
        };

        if (grossPayout > 0m)
            postings.Add(new LedgerPosting(playerChipAccount, currency, grossPayout));

        if (grossPayout > wagerAmount)
        {
            RequireDifferentAccounts(tableEscrowAccount, houseReserveAccount, nameof(tableEscrowAccount), nameof(houseReserveAccount));
            RequireDifferentAccounts(playerChipAccount, houseReserveAccount, nameof(playerChipAccount), nameof(houseReserveAccount));
            postings.Add(new LedgerPosting(houseReserveAccount, currency, -(grossPayout - wagerAmount)));
        }
        else if (grossPayout < wagerAmount)
        {
            RequireDifferentAccounts(tableEscrowAccount, houseGamingRevenueAccount, nameof(tableEscrowAccount), nameof(houseGamingRevenueAccount));
            RequireDifferentAccounts(playerChipAccount, houseGamingRevenueAccount, nameof(playerChipAccount), nameof(houseGamingRevenueAccount));
            postings.Add(new LedgerPosting(houseGamingRevenueAccount, currency, wagerAmount - grossPayout));
        }

        return ledger.Post(new LedgerTransaction(transactionId, "wager-settlement", occurredAt, postings));
    }

    private static void RequirePositive(decimal amount, string parameterName)
    {
        if (amount <= 0m)
            throw new ArgumentOutOfRangeException(parameterName, "Amount must be greater than zero.");
    }

    private static string NormalizeCurrency(string currency, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be blank.", parameterName);

        return currency.Trim().ToUpperInvariant();
    }

    private static void RequireDifferentCurrencies(string first, string second, string parameterName)
    {
        if (string.Equals(first, second, StringComparison.Ordinal))
            throw new ArgumentException("Cash and chip currency codes must differ.", parameterName);
    }

    private static void RequireDifferentAccounts(
        string first,
        string second,
        string firstParameterName,
        string secondParameterName)
    {
        if (string.IsNullOrWhiteSpace(first))
            throw new ArgumentException("Account ID cannot be blank.", firstParameterName);
        if (string.IsNullOrWhiteSpace(second))
            throw new ArgumentException("Account ID cannot be blank.", secondParameterName);
        if (string.Equals(first, second, StringComparison.Ordinal))
            throw new ArgumentException("This operation requires distinct account IDs.", secondParameterName);
    }
}
