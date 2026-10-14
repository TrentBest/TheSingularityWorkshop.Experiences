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
