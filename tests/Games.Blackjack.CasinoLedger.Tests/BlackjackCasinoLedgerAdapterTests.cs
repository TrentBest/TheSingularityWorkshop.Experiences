using TheSingularityWorkshop.Games.Blackjack;
using TheSingularityWorkshop.Games.Blackjack.CasinoLedger;
using TheSingularityWorkshop.Games.CasinoLedger;
using Xunit;

namespace TheSingularityWorkshop.Games.Blackjack.CasinoLedger.Tests;

public sealed class BlackjackCasinoLedgerAdapterTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 8, 0, 0, TimeSpan.Zero);

    private static (CasinoLedger Ledger, BlackjackWagerBook Book, BlackjackCasinoLedgerAdapter Adapter) CreateSystem()
    {
        var ledger = new CasinoLedger();
        ledger.RegisterAccount("player");
        ledger.RegisterAccount("table-escrow");
        ledger.RegisterAccount("house-reserve");
        ledger.RegisterAccount("house-revenue");
        ledger.RegisterAccount("virtual-issuance", allowNegativeBalance: true);
        ledger.Post(new LedgerTransaction("seed-virtual-chips", "simulation-funding", Now,
        [
            new LedgerPosting("virtual-issuance", "CHIP", -200m),
            new LedgerPosting("player", "CHIP", 100m),
            new LedgerPosting("house-reserve", "CHIP", 100m)
        ]));
        return (ledger, new BlackjackWagerBook(), new BlackjackCasinoLedgerAdapter());
    }

    [Fact]
    public void Placement_and_win_settlement_reconcile_and_are_idempotent()
    {
        var (ledger, book, adapter) = CreateSystem();
        Assert.True(adapter.PlaceWager(ledger, book, "round-1:hand-0", "place-1", Now,
            "player", "table-escrow", 10m));
        Assert.False(adapter.PlaceWager(ledger, book, "round-1:hand-0", "place-1-retry", Now,
            "player", "table-escrow", 10m));
        Assert.Equal(90m, ledger.GetBalance("player", "CHIP"));
        Assert.Equal(10m, ledger.GetBalance("table-escrow", "CHIP"));

        Assert.True(adapter.SettleWager(ledger, book, "round-1:hand-0", "settle-1", Now,
            "table-escrow", "player", "house-reserve", "house-revenue", 20m));
        Assert.False(adapter.SettleWager(ledger, book, "round-1:hand-0", "settle-1", Now,
            "table-escrow", "player", "house-reserve", "house-revenue", 20m));
        Assert.Equal(110m, ledger.GetBalance("player", "CHIP"));
        Assert.Equal(0m, ledger.GetBalance("table-escrow", "CHIP"));
        Assert.Equal(90m, ledger.GetBalance("house-reserve", "CHIP"));
        Assert.True(ledger.Reconcile().IsBalanced);
    }

    [Fact]
    public void Different_transaction_id_cannot_settle_same_logical_wager_twice()
    {
        var (ledger, book, adapter) = CreateSystem();
        adapter.PlaceWager(ledger, book, "round-2:hand-0", "place-2", Now,
            "player", "table-escrow", 10m);
        adapter.SettleWager(ledger, book, "round-2:hand-0", "settle-2a", Now,
            "table-escrow", "player", "house-reserve", "house-revenue", 0m);

        Assert.Throws<InvalidOperationException>(() =>
            adapter.SettleWager(ledger, book, "round-2:hand-0", "settle-2b", Now,
                "table-escrow", "player", "house-reserve", "house-revenue", 0m));
        Assert.Equal(1, ledger.GetTransactions().Count(transaction => transaction.Kind == "wager-settlement"));
        Assert.True(ledger.Reconcile().IsBalanced);
    }

    [Fact]
    public void Losing_wager_credits_house_revenue_and_reconciles()
    {
        var (ledger, book, adapter) = CreateSystem();
        adapter.PlaceWager(ledger, book, "round-3:hand-0", "place-3", Now,
            "player", "table-escrow", 15m);
        adapter.SettleWager(ledger, book, "round-3:hand-0", "settle-3", Now,
            "table-escrow", "player", "house-reserve", "house-revenue", 0m);

        Assert.Equal(15m, ledger.GetBalance("house-revenue", "CHIP"));
        Assert.Equal(0m, ledger.GetBalance("table-escrow", "CHIP"));
        Assert.True(ledger.Reconcile().IsBalanced);
    }

    [Fact]
    public void Failed_ledger_settlement_leaves_wager_open_for_retry()
    {
        var (ledger, book, adapter) = CreateSystem();
        adapter.PlaceWager(ledger, book, "round-4:hand-0", "place-4", Now,
            "player", "table-escrow", 10m);

        Assert.Throws<InvalidOperationException>(() =>
            adapter.SettleWager(ledger, book, "round-4:hand-0", "settle-4", Now,
                "table-escrow", "player", "missing-reserve", "house-revenue", 20m));
        Assert.Equal(BlackjackWagerStatus.Open, book.Get("round-4:hand-0")!.Status);
        Assert.Equal(10m, ledger.GetBalance("table-escrow", "CHIP"));

        Assert.True(adapter.SettleWager(ledger, book, "round-4:hand-0", "settle-4-retry", Now,
            "table-escrow", "player", "house-reserve", "house-revenue", 20m));
        Assert.True(ledger.Reconcile().IsBalanced);
    }
    [Fact]
    public void Double_down_additional_stake_is_escrowed_once_and_settled_as_one_hand_wager()
    {
        var (ledger, book, adapter) = CreateSystem();
        Assert.True(adapter.PlaceWager(ledger, book, "round-5:hand-0", "place-5", Now,
            "player", "table-escrow", 10m));
        Assert.True(adapter.AddStake(ledger, book, "round-5:hand-0", "double-5", Now,
            "player", "table-escrow", 10m));
        Assert.False(adapter.AddStake(ledger, book, "round-5:hand-0", "double-5", Now,
            "player", "table-escrow", 10m));

        Assert.Equal(20m, book.Get("round-5:hand-0")!.Amount);
        Assert.Equal(20m, ledger.GetBalance("table-escrow", "CHIP"));
        Assert.True(adapter.SettleWager(ledger, book, "round-5:hand-0", "settle-5", Now,
            "table-escrow", "player", "house-reserve", "house-revenue", 40m));

        Assert.Equal(120m, ledger.GetBalance("player", "CHIP"));
        Assert.Equal(80m, ledger.GetBalance("house-reserve", "CHIP"));
        Assert.Equal(0m, ledger.GetBalance("table-escrow", "CHIP"));
        Assert.True(ledger.Reconcile().IsBalanced);
    }

}
