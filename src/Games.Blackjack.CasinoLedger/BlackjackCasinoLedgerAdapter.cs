using TheSingularityWorkshop.Games.Blackjack;
using TheSingularityWorkshop.Games.CasinoLedger;

namespace TheSingularityWorkshop.Games.Blackjack.CasinoLedger;

/// <summary>
/// Explicit bridge between logical Blackjack wager lifecycle and the simulation-only CasinoLedger.
/// </summary>
/// <remarks>
/// This adapter keeps accounting outside the Blackjack rules/round assembly. It posts only virtual chip
/// movements and relies on a caller-configured ledger chart of accounts. The wager book is process-local;
/// this adapter does not provide durable or distributed exactly-once guarantees.
/// </remarks>
public sealed class BlackjackCasinoLedgerAdapter
{
    /// <summary>
    /// Posts a wager into table escrow once for its stable logical wager identity.
    /// </summary>
    /// <returns>True when a new ledger posting was created; false for an already registered wager or exact ledger replay.</returns>
    public bool PlaceWager(
        CasinoLedger ledger,
        BlackjackWagerBook wagerBook,
        string wagerId,
        string placementTransactionId,
        DateTimeOffset occurredAt,
        string playerChipAccount,
        string tableEscrowAccount,
        decimal amount,
        string chipCurrency = "CHIP")
    {
        ArgumentNullException.ThrowIfNull(ledger);
        ArgumentNullException.ThrowIfNull(wagerBook);
        return wagerBook.RegisterOnce(wagerId, amount, () =>
            CasinoLedgerOperations.PlaceWager(ledger, placementTransactionId, occurredAt,
                playerChipAccount, tableEscrowAccount, amount, chipCurrency));
    }

    /// <summary>
    /// Escrows an additional stake for an existing open hand, such as the extra stake on double down.
    /// </summary>
    /// <returns>True when a new ledger posting was created; false for an exact replay.</returns>
    public bool AddStake(
        CasinoLedger ledger,
        BlackjackWagerBook wagerBook,
        string wagerId,
        string transactionId,
        DateTimeOffset occurredAt,
        string playerChipAccount,
        string tableEscrowAccount,
        decimal amount,
        string chipCurrency = "CHIP")
    {
        ArgumentNullException.ThrowIfNull(ledger);
        ArgumentNullException.ThrowIfNull(wagerBook);
        return wagerBook.IncreaseStakeOnce(wagerId, transactionId, amount, occurredAt, () =>
            CasinoLedgerOperations.PlaceWager(ledger, transactionId, occurredAt,
                playerChipAccount, tableEscrowAccount, amount, chipCurrency));
    }

    /// <summary>
    /// Applies one ordered round commitment using deterministic IDs derived from the caller's round identity.
    /// </summary>
    /// <remarks>
    /// Initial and split commitments create distinct hand wagers. A double-down commitment adds stake to the
    /// existing wager for that stable hand ID.
    /// </remarks>
    public bool ApplyCommitment(
        CasinoLedger ledger,
        BlackjackWagerBook wagerBook,
        string roundId,
        BlackjackWagerCommitment commitment,
        DateTimeOffset occurredAt,
        string playerChipAccount,
        string tableEscrowAccount,
        string chipCurrency = "CHIP")
    {
        ArgumentNullException.ThrowIfNull(ledger);
        ArgumentNullException.ThrowIfNull(wagerBook);
        ArgumentNullException.ThrowIfNull(commitment);
        if (string.IsNullOrWhiteSpace(roundId))
            throw new ArgumentException("Round ID cannot be blank.", nameof(roundId));

        var wagerId = $"blackjack:{roundId}:hand:{commitment.HandId}";
        var transactionId = $"blackjack:{roundId}:commit:{commitment.Sequence}";
        return commitment.Kind switch
        {
            BlackjackWagerCommitmentKind.Initial or BlackjackWagerCommitmentKind.Split =>
                PlaceWager(ledger, wagerBook, wagerId, transactionId, occurredAt,
                    playerChipAccount, tableEscrowAccount, commitment.Amount, chipCurrency),
            BlackjackWagerCommitmentKind.DoubleDown =>
                AddStake(ledger, wagerBook, wagerId, transactionId, occurredAt,
                    playerChipAccount, tableEscrowAccount, commitment.Amount, chipCurrency),
            _ => throw new ArgumentOutOfRangeException(nameof(commitment), "Unknown wager commitment kind.")
        };
    }

    /// <summary>
    /// Settles a registered Blackjack wager once, regardless of subsequent transaction-ID changes.
    /// </summary>
    /// <returns>True when a new ledger posting was created; false for an exact replay.</returns>
    public bool SettleWager(
        CasinoLedger ledger,
        BlackjackWagerBook wagerBook,
        string wagerId,
        string settlementTransactionId,
        DateTimeOffset occurredAt,
        string tableEscrowAccount,
        string playerChipAccount,
        string houseReserveAccount,
        string houseGamingRevenueAccount,
        decimal grossReturn,
        string chipCurrency = "CHIP")
    {
        ArgumentNullException.ThrowIfNull(ledger);
        ArgumentNullException.ThrowIfNull(wagerBook);
        var wager = wagerBook.Get(wagerId)
            ?? throw new ArgumentException("Wager identity is not registered.", nameof(wagerId));

        return wagerBook.SettleOnce(wagerId, settlementTransactionId, grossReturn, occurredAt, () =>
            CasinoLedgerOperations.SettleWager(ledger, settlementTransactionId, occurredAt,
                tableEscrowAccount, playerChipAccount, houseReserveAccount, houseGamingRevenueAccount,
                wager.Amount, grossReturn, chipCurrency));
    }
}
