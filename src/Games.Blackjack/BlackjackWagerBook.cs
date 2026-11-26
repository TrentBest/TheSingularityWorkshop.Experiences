namespace TheSingularityWorkshop.Games.Blackjack;

/// <summary>The lifecycle state of a logical Blackjack wager.</summary>
public enum BlackjackWagerStatus
{
    /// <summary>The wager has been recorded but has not been settled.</summary>
    Open,
    /// <summary>The wager has one accepted settlement.</summary>
    Settled
}

/// <summary>An immutable snapshot of a logical Blackjack wager and its settlement identity.</summary>
/// <param name="WagerId">Stable logical identity for this wager, independent of ledger transaction IDs.</param>
/// <param name="Amount">Original virtual wager amount.</param>
/// <param name="Status">Current lifecycle state.</param>
/// <param name="SettlementTransactionId">The one accepted ledger transaction ID, if settled.</param>
/// <param name="GrossReturn">The accepted gross virtual return, including returned stake.</param>
/// <param name="SettledAt">The accepted settlement timestamp.</param>
public sealed record BlackjackWagerRecord(
    string WagerId,
    decimal Amount,
    BlackjackWagerStatus Status,
    string? SettlementTransactionId = null,
    decimal? GrossReturn = null,
    DateTimeOffset? SettledAt = null);

/// <summary>
/// In-memory, thread-safe logical wager lifecycle that prevents a wager from being settled twice
/// under different ledger transaction IDs.
/// </summary>
/// <remarks>
/// The book is a domain guard, not a ledger or durable store. The supplied settlement callback should
/// post the corresponding balanced virtual-ledger transaction and return true when newly posted or
/// false when the identical transaction was already posted. A callback exception leaves the wager open.
/// State is process-local; durable recovery and cross-process concurrency require a persistent coordinator.
/// </remarks>
public sealed class BlackjackWagerBook
{
    private readonly object _gate = new();
    private readonly Dictionary<string, BlackjackWagerRecord> _wagers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _settlementOwners = new(StringComparer.Ordinal);

    /// <summary>Registers a wager identity. Re-registering the same ID and amount is idempotent.</summary>
    /// <exception cref="ArgumentException">The ID is blank or already exists with a different amount.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The amount is not positive.</exception>
    public BlackjackWagerRecord Register(string wagerId, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(wagerId))
            throw new ArgumentException("Wager ID cannot be blank.", nameof(wagerId));
        if (amount <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "Wager amount must be positive.");

        lock (_gate)
        {
            if (_wagers.TryGetValue(wagerId, out var existing))
            {
                if (existing.Amount != amount)
                    throw new ArgumentException("Wager ID is already registered with a different amount.", nameof(wagerId));
                return existing;
            }

            var record = new BlackjackWagerRecord(wagerId, amount, BlackjackWagerStatus.Open);
            _wagers.Add(wagerId, record);
            return record;
        }
    }

    /// <summary>Gets an immutable wager snapshot, or null when the identity is unknown.</summary>
    public BlackjackWagerRecord? Get(string wagerId)
    {
        if (string.IsNullOrWhiteSpace(wagerId))
            throw new ArgumentException("Wager ID cannot be blank.", nameof(wagerId));
        lock (_gate)
            return _wagers.GetValueOrDefault(wagerId);
    }

    /// <summary>
    /// Executes and records the one accepted settlement for a wager.
    /// </summary>
    /// <param name="wagerId">The stable logical wager identity.</param>
    /// <param name="settlementTransactionId">The unique ledger transaction ID for this settlement.</param>
    /// <param name="grossReturn">Gross virtual return, including returned stake.</param>
    /// <param name="settledAt">Caller-supplied deterministic settlement timestamp.</param>
    /// <param name="postSettlement">Callback that posts the balanced ledger transaction; false means an identical transaction already exists.</param>
    /// <returns>True when the callback newly posted the ledger transaction; false for an exact replay.</returns>
    /// <exception cref="ArgumentException">An identifier is blank, the wager is unknown, or a settled wager is reused with different settlement data.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Gross return is negative.</exception>
    /// <exception cref="InvalidOperationException">The transaction ID belongs to another wager or the wager has a conflicting settlement.</exception>
    /// <remarks>The callback runs under the book's lock to serialize competing settlement attempts in this process.</remarks>
    public bool SettleOnce(
        string wagerId,
        string settlementTransactionId,
        decimal grossReturn,
        DateTimeOffset settledAt,
        Func<bool> postSettlement)
    {
        if (string.IsNullOrWhiteSpace(wagerId))
            throw new ArgumentException("Wager ID cannot be blank.", nameof(wagerId));
        if (string.IsNullOrWhiteSpace(settlementTransactionId))
            throw new ArgumentException("Settlement transaction ID cannot be blank.", nameof(settlementTransactionId));
        if (grossReturn < 0m)
            throw new ArgumentOutOfRangeException(nameof(grossReturn), "Gross return cannot be negative.");
        if (settledAt == default)
            throw new ArgumentException("Settlement timestamp must be supplied.", nameof(settledAt));
        ArgumentNullException.ThrowIfNull(postSettlement);

        lock (_gate)
        {
            if (!_wagers.TryGetValue(wagerId, out var existing))
                throw new ArgumentException("Wager identity is not registered.", nameof(wagerId));

            if (existing.Status == BlackjackWagerStatus.Settled)
            {
                if (existing.SettlementTransactionId == settlementTransactionId &&
                    existing.GrossReturn == grossReturn &&
                    existing.SettledAt == settledAt)
                    return false;
                throw new InvalidOperationException("This logical wager already has a different accepted settlement.");
            }

            if (_settlementOwners.TryGetValue(settlementTransactionId, out var owner) && owner != wagerId)
                throw new InvalidOperationException("Settlement transaction ID is already assigned to another wager.");

            var newlyPosted = postSettlement();
            _settlementOwners.Add(settlementTransactionId, wagerId);
            _wagers[wagerId] = existing with
            {
                Status = BlackjackWagerStatus.Settled,
                SettlementTransactionId = settlementTransactionId,
                GrossReturn = grossReturn,
                SettledAt = settledAt
            };
            return newlyPosted;
        }
    }
}
