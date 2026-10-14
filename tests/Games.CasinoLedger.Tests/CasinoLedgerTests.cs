using TheSingularityWorkshop.Games.CasinoLedger;
using Xunit;

namespace TheSingularityWorkshop.Games.CasinoLedger.Tests;

public sealed class CasinoLedgerTests
{
    private static readonly DateTimeOffset EventTime = new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Post_commits_balanced_transfer_and_reconciles()
    {
        var ledger = CreateLedger("clearing", "player", "cage");
        var transaction = Transaction("fund-player", Posting("clearing", "s$", -50m), Posting("player", "S$", 50m));

        Assert.True(ledger.Post(transaction));
        Assert.Equal(50m, ledger.GetBalance("player", "S$"));
        Assert.Equal(-50m, ledger.GetBalance("clearing", "S$"));
        Assert.True(ledger.Reconcile().IsBalanced);
        Assert.Equal(1, ledger.GetTransactions().Count);
    }

    [Fact]
    public void Post_rejects_unbalanced_transaction_without_mutating_state()
    {
        var ledger = CreateLedger("player", "cage");
        var transaction = Transaction("bad", Posting("player", "CHIP", 5m), Posting("cage", "CHIP", 4m));

        Assert.Throws<InvalidOperationException>(() => ledger.Post(transaction));
        Assert.Empty(ledger.GetTransactions());
        Assert.Empty(ledger.GetBalances());
    }

    [Fact]
    public void Post_rejects_overdraft_atomically()
    {
        var ledger = CreateLedger("player", "cage", "house");
        Assert.True(ledger.Post(Transaction("fund-player", Posting("house", "CHIP", -20m), Posting("player", "CHIP", 20m))));
        var failed = Transaction("overdraw", Posting("player", "CHIP", -25m), Posting("cage", "CHIP", 25m));

        Assert.Throws<InvalidOperationException>(() => ledger.Post(failed));
        Assert.Equal(20m, ledger.GetBalance("player", "CHIP"));
        Assert.Equal(0m, ledger.GetBalance("cage", "CHIP"));
        Assert.Equal(1, ledger.GetTransactions().Count);
    }

    [Fact]
    public void Multi_currency_buy_in_balances_each_currency_independently()
    {
        var ledger = CreateLedger("player-cash", "cage-cash", "cage-chips", "player-chips", "mint");
        Assert.True(ledger.Post(Transaction("fund-cash", Posting("mint", "S$", -100m), Posting("player-cash", "S$", 100m))));
        Assert.True(ledger.Post(Transaction("fund-chips", Posting("mint", "CHIP", -100m), Posting("cage-chips", "CHIP", 100m))));

        var buyIn = Transaction("buy-in", Posting("player-cash", "S$", -30m),
            Posting("cage-cash", "S$", 30m), Posting("cage-chips", "CHIP", -30m),
            Posting("player-chips", "CHIP", 30m));

        Assert.True(ledger.Post(buyIn));
        Assert.Equal(70m, ledger.GetBalance("player-cash", "S$"));
        Assert.Equal(30m, ledger.GetBalance("cage-cash", "S$"));
        Assert.Equal(70m, ledger.GetBalance("cage-chips", "CHIP"));
        Assert.Equal(30m, ledger.GetBalance("player-chips", "CHIP"));
        Assert.True(ledger.Reconcile().IsBalanced);
        Assert.Equal(0m, ledger.Reconcile().TotalsByCurrency["S$"]);
        Assert.Equal(0m, ledger.Reconcile().TotalsByCurrency["CHIP"]);
    }

    [Fact]
    public void Repeated_postings_to_one_account_are_validated_as_one_atomic_change()
    {
        var ledger = CreateLedger("player");
        var transaction = Transaction("net-zero", Posting("player", "CHIP", -10m), Posting("player", "CHIP", 10m));

        Assert.True(ledger.Post(transaction));
        Assert.Equal(0m, ledger.GetBalance("player", "CHIP"));
        Assert.True(ledger.Reconcile().IsBalanced);
    }

    [Fact]
    public void Identical_replay_is_idempotent()
    {
        var ledger = CreateLedger("a", "b");
        var transaction = Transaction("event-1", Posting("a", "CHIP", -1m), Posting("b", "CHIP", 1m));

        Assert.True(ledger.Post(transaction));
        Assert.False(ledger.Post(Transaction("event-1", Posting("a", "CHIP", -1m), Posting("b", "CHIP", 1m))));
        Assert.Equal(1, ledger.GetTransactions().Count);
    }

    [Fact]
    public void Reusing_transaction_id_for_different_content_is_rejected()
    {
        var ledger = CreateLedger("a", "b", "c");
        Assert.True(ledger.Post(Transaction("event-1", Posting("a", "CHIP", -1m), Posting("b", "CHIP", 1m))));

        var different = Transaction("event-1", Posting("a", "CHIP", -2m), Posting("c", "CHIP", 2m));
        Assert.Throws<InvalidOperationException>(() => ledger.Post(different));
        Assert.Equal(1, ledger.GetTransactions().Count);
    }

    [Fact]
    public void Unknown_account_is_rejected_without_partial_commit()
    {
        var ledger = CreateLedger("known");
        var transaction = Transaction("unknown", Posting("known", "CHIP", -1m), Posting("missing", "CHIP", 1m));

        Assert.Throws<InvalidOperationException>(() => ledger.Post(transaction));
        Assert.Empty(ledger.GetTransactions());
        Assert.Empty(ledger.GetBalances());
    }

    [Fact]
    public void Posting_and_transaction_inputs_are_snapshotted()
    {
        var ledger = CreateLedger("a", "b");
        var postings = new List<LedgerPosting> { Posting("a", "CHIP", -2m), Posting("b", "CHIP", 2m) };
        var transaction = new LedgerTransaction("snapshot", "test", EventTime, postings);
        postings.Clear();

        Assert.Equal(2, transaction.Postings.Count);
        Assert.True(ledger.Post(transaction));
        Assert.Equal(2, ledger.GetTransactions()[0].Postings.Count);
        Assert.Throws<NotSupportedException>(() => ((IList<LedgerPosting>)transaction.Postings).Clear());
        Assert.Throws<NotSupportedException>(() => ((IDictionary<(string AccountId, string Currency), decimal>)ledger.GetBalances()).Clear());
    }

    [Fact]
    public void Account_ids_are_unique_and_currency_codes_are_normalized()
    {
        var ledger = CreateLedger("clearing", "player", "cage");
        Assert.Throws<ArgumentException>(() => ledger.RegisterAccount("player"));
        Assert.True(ledger.Post(Transaction("fund", Posting("clearing", "chip", -3m), Posting("player", "CHIP", 3m))));
        Assert.Equal(3m, ledger.GetBalance("player", "chip"));
        Assert.Equal(-3m, ledger.GetBalance("clearing", "CHIP"));
    }

    [Fact]
    public void Concurrent_balanced_posts_do_not_lose_updates()
    {
        var ledger = CreateLedger("clearing", "player");
        Parallel.For(0, 250, index =>
        {
            ledger.Post(Transaction($"event-{index}", Posting("clearing", "CHIP", -1m), Posting("player", "CHIP", 1m)));
        });

        Assert.Equal(250m, ledger.GetBalance("player", "CHIP"));
        Assert.Equal(-250m, ledger.GetBalance("clearing", "CHIP"));
        Assert.Equal(250, ledger.GetTransactions().Count);
        Assert.True(ledger.Reconcile().IsBalanced);
    }

    [Fact]
    public void Transaction_rejects_zero_postings_and_missing_identifiers()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Posting("a", "CHIP", 0m));
        Assert.Throws<ArgumentException>(() => new LedgerTransaction(" ", "test", EventTime,
            [Posting("a", "CHIP", -1m), Posting("b", "CHIP", 1m)]));
        Assert.Throws<ArgumentException>(() => new LedgerTransaction("id", "test", EventTime, [Posting("a", "CHIP", 1m)]));
    }

    private static CasinoLedger CreateLedger(params string[] accountIds)
    {
        var ledger = new CasinoLedger();
        foreach (var accountId in accountIds)
            ledger.RegisterAccount(accountId, allowNegativeBalance: accountId is "mint" or "house" or "clearing" or "a" or "b");
        return ledger;
    }

    private static LedgerPosting Posting(string accountId, string currency, decimal amount) =>
        new(accountId, currency, amount);

    private static LedgerTransaction Transaction(string id, params LedgerPosting[] postings) =>
        new(id, "test", EventTime, postings);
}
