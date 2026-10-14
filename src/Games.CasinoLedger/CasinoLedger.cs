using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.Games.CasinoLedger;

/// <summary>
/// Thread-safe, in-memory ledger for simulation-only casino accounting.
/// </summary>
/// <remarks>
/// Transactions are atomic, immutable after posting, and balanced per currency. The ledger does not assign economic
/// meaning to account names or transaction kinds; the caller defines the chart of accounts and business rules.
/// </remarks>
public sealed class CasinoLedger
{
    private readonly object _gate = new();
    private readonly Dictionary<string, LedgerAccount> _accounts = new(StringComparer.Ordinal);
    private readonly Dictionary<(string AccountId, string Currency), decimal> _balances = new();
    private readonly Dictionary<string, LedgerTransaction> _transactions = new(StringComparer.Ordinal);
    private readonly List<LedgerTransaction> _journal = new();

    /// <summary>Registers an account before it can receive postings.</summary>
    /// <exception cref="ArgumentException">The ID is blank or already registered.</exception>
    public void RegisterAccount(string accountId, bool allowNegativeBalance = false)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID cannot be blank.", nameof(accountId));

        lock (_gate)
        {
            if (!_accounts.TryAdd(accountId, new LedgerAccount(accountId, allowNegativeBalance)))
                throw new ArgumentException($"Account '{accountId}' is already registered.", nameof(accountId));
        }
    }

    /// <summary>Posts a balanced transaction atomically.</summary>
    /// <returns><see langword="true"/> if committed; <see langword="false"/> if the identical transaction was already committed.</returns>
    /// <exception cref="ArgumentNullException">The transaction is null.</exception>
    /// <exception cref="InvalidOperationException">The transaction is unbalanced, references an unknown account, overdrafts an account, or reuses an ID with different content.</exception>
    public bool Post(LedgerTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        lock (_gate)
        {
            if (_transactions.TryGetValue(transaction.TransactionId, out var existing))
            {
                if (AreEquivalent(existing, transaction))
                    return false;

                throw new InvalidOperationException(
                    $"Transaction ID '{transaction.TransactionId}' was already used for different content.");
            }

            var totals = new Dictionary<string, decimal>(StringComparer.Ordinal);
            var changes = new Dictionary<(string AccountId, string Currency), decimal>();

            foreach (var posting in transaction.Postings)
            {
                if (!_accounts.TryGetValue(posting.AccountId, out var account))
                    throw new InvalidOperationException($"Account '{posting.AccountId}' is not registered.");

                totals.TryGetValue(posting.Currency, out var currencyTotal);
                totals[posting.Currency] = checked(currencyTotal + posting.Amount);

                var key = (posting.AccountId, posting.Currency);
                changes.TryGetValue(key, out var previousChange);
                changes[key] = checked(previousChange + posting.Amount);

                _balances.TryGetValue(key, out var currentBalance);
                var projectedBalance = checked(currentBalance + changes[key]);
                if (!account.AllowNegativeBalance && projectedBalance < 0m)
                {
                    throw new InvalidOperationException(
                        $"Transaction '{transaction.TransactionId}' would overdraw account '{posting.AccountId}' in {posting.Currency}.");
                }
            }

            var unbalanced = totals.FirstOrDefault(pair => pair.Value != 0m);
            if (!string.IsNullOrEmpty(unbalanced.Key))
            {
                throw new InvalidOperationException(
                    $"Transaction '{transaction.TransactionId}' is not balanced in {unbalanced.Key}: net {unbalanced.Value}.");
            }

            foreach (var change in changes)
            {
                _balances.TryGetValue(change.Key, out var currentBalance);
                _balances[change.Key] = checked(currentBalance + change.Value);
            }

            _transactions.Add(transaction.TransactionId, transaction);
            _journal.Add(transaction);
            return true;
        }
    }

    /// <summary>Gets the current balance of a registered account in the requested currency.</summary>
    /// <exception cref="ArgumentException">The account is not registered or currency is blank.</exception>
    public decimal GetBalance(string accountId, string currency)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID cannot be blank.", nameof(accountId));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be blank.", nameof(currency));

        lock (_gate)
        {
            if (!_accounts.ContainsKey(accountId))
                throw new ArgumentException($"Account '{accountId}' is not registered.", nameof(accountId));

            return _balances.TryGetValue((accountId, currency.Trim().ToUpperInvariant()), out var balance)
                ? balance
                : 0m;
        }
    }

    /// <summary>Gets a stable snapshot of the committed journal in commit order.</summary>
    public IReadOnlyList<LedgerTransaction> GetTransactions()
    {
        lock (_gate)
            return Array.AsReadOnly(_journal.ToArray());
    }

    /// <summary>Gets an immutable snapshot of all nonzero account balances.</summary>
    public IReadOnlyDictionary<(string AccountId, string Currency), decimal> GetBalances()
    {
        lock (_gate)
            return new ReadOnlyDictionary<(string AccountId, string Currency), decimal>(
                new Dictionary<(string AccountId, string Currency), decimal>(_balances));
    }

    /// <summary>Reconciles the current ledger by summing account balances independently for each currency.</summary>
    public ReconciliationReport Reconcile()
    {
        lock (_gate)
        {
            var totals = new Dictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var balance in _balances)
            {
                totals.TryGetValue(balance.Key.Currency, out var current);
                totals[balance.Key.Currency] = checked(current + balance.Value);
            }

            return new ReconciliationReport(totals);
        }
    }

    private static bool AreEquivalent(LedgerTransaction left, LedgerTransaction right)
    {
        if (left.Kind != right.Kind ||
            left.OccurredAt != right.OccurredAt ||
            left.Postings.Count != right.Postings.Count)
            return false;

        for (var i = 0; i < left.Postings.Count; i++)
        {
            if (left.Postings[i] != right.Postings[i])
                return false;
        }

        return true;
    }
}
