using TheSingularityWorkshop.Games.Blackjack;
using Xunit;

namespace TheSingularityWorkshop.Games.Blackjack.Tests;

public sealed class BlackjackWagerBookTests
{
    private static readonly DateTimeOffset SettledAt =
        new(2026, 11, 26, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Register_is_idempotent_for_same_identity_and_amount()
    {
        var book = new BlackjackWagerBook();
        var callbackCount = 0;
        var first = book.RegisterOnce("round-1:hand-0", 10m, () => { callbackCount++; return true; });
        var second = book.RegisterOnce("round-1:hand-0", 10m, () => { callbackCount++; return true; });
        Assert.True(first);
        Assert.False(second);
        Assert.Equal(1, callbackCount);
        Assert.Equal(BlackjackWagerStatus.Open, book.Get("round-1:hand-0")!.Status);
    }

    [Fact]
    public void Register_rejects_reuse_of_identity_with_different_amount()
    {
        var book = new BlackjackWagerBook();
        book.RegisterOnce("round-1:hand-0", 10m, () => true);
        Assert.Throws<ArgumentException>(() => book.Register("round-1:hand-0", 20m));
    }

    [Fact]
    public void SettleOnce_posts_only_once_even_when_retried_with_a_different_call()
    {
        var book = new BlackjackWagerBook();
        book.Register("round-1:hand-0", 10m);
        var postCount = 0;

        var posted = book.SettleOnce("round-1:hand-0", "settle-round-1-hand-0", 25m, SettledAt,
            () => { postCount++; return true; });
        var replay = book.SettleOnce("round-1:hand-0", "settle-round-1-hand-0", 25m, SettledAt,
            () => { postCount++; return true; });

        Assert.True(posted);
        Assert.False(replay);
        Assert.Equal(1, postCount);
        Assert.Equal(BlackjackWagerStatus.Settled, book.Get("round-1:hand-0")!.Status);
        Assert.Equal(25m, book.Get("round-1:hand-0")!.GrossReturn);
    }

    [Fact]
    public void SettleOnce_rejects_different_transaction_id_for_already_settled_wager()
    {
        var book = new BlackjackWagerBook();
        book.Register("round-1:hand-0", 10m);
        book.SettleOnce("round-1:hand-0", "settlement-a", 20m, SettledAt, () => true);

        Assert.Throws<InvalidOperationException>(() =>
            book.SettleOnce("round-1:hand-0", "settlement-b", 20m, SettledAt, () => true));
    }

    [Fact]
    public void SettleOnce_rejects_one_transaction_id_claimed_by_two_wagers()
    {
        var book = new BlackjackWagerBook();
        book.Register("round-1:hand-0", 10m);
        book.RegisterOnce("round-1:hand-1", 10m, () => true);
        book.SettleOnce("round-1:hand-0", "shared-settlement", 20m, SettledAt, () => true);

        Assert.Throws<InvalidOperationException>(() =>
            book.SettleOnce("round-1:hand-1", "shared-settlement", 20m, SettledAt, () => true));
    }

    [Fact]
    public void Failed_settlement_callback_leaves_wager_open_for_retry()
    {
        var book = new BlackjackWagerBook();
        book.Register("round-1:hand-0", 10m);

        Assert.Throws<InvalidOperationException>(() =>
            book.SettleOnce("round-1:hand-0", "settlement-a", 20m, SettledAt,
                () => throw new InvalidOperationException("Ledger rejected the posting.")));

        Assert.Equal(BlackjackWagerStatus.Open, book.Get("round-1:hand-0")!.Status);
        Assert.True(book.SettleOnce("round-1:hand-0", "settlement-a", 20m, SettledAt, () => true));
    }
    [Fact]
    public void IncreaseStakeOnce_updates_total_once_and_rejects_increases_after_settlement()
    {
        var book = new BlackjackWagerBook();
        book.RegisterOnce("round-5:hand-0", 10m, () => true);
        var postCount = 0;
        Assert.True(book.IncreaseStakeOnce("round-5:hand-0", "double-5", 10m, SettledAt,
            () => { postCount++; return true; }));
        Assert.False(book.IncreaseStakeOnce("round-5:hand-0", "double-5", 10m, SettledAt,
            () => { postCount++; return true; }));
        Assert.Equal(20m, book.Get("round-5:hand-0")!.Amount);
        Assert.Equal(1, postCount);

        book.SettleOnce("round-5:hand-0", "settle-5", 40m, SettledAt, () => true);
        Assert.Throws<InvalidOperationException>(() =>
            book.IncreaseStakeOnce("round-5:hand-0", "late-double", 10m, SettledAt, () => true));
    }

}
