using TheSingularityWorkshop.Experiences.Minesweeper;
using Xunit;

namespace TheSingularityWorkshop.Experiences.Minesweeper.Tests;

public sealed class MinesweeperGameTests
{
    [Fact]
    public void SameSeedProducesSameMineLayoutAndNeighborCounts()
    {
        var first = new MinesweeperGame(9, 9, 10, seed: 123);
        var second = new MinesweeperGame(9, 9, 10, seed: 123);
        Assert.Equal(first.Snapshot().Select(c => (c.Id, c.AdjacentMines)), second.Snapshot().Select(c => (c.Id, c.AdjacentMines)));
    }

    [Fact]
    public void FixedLayoutComputesNeighborCounts()
    {
        var game = new MinesweeperGame(3, 3, new[] { new CellId(0, 0), new CellId(2, 2) });
        Assert.Equal(2, game.GetCell(new CellId(1, 1)).AdjacentMines);
        Assert.Equal(1, game.GetCell(new CellId(0, 1)).AdjacentMines);
        Assert.Equal(0, game.GetCell(new CellId(0, 2)).AdjacentMines);
    }

    [Fact]
    public void FirstRevealRelocatesMineAndNeverLoses()
    {
        var game = new MinesweeperGame(3, 1, new[] { new CellId(0, 0) });
        Assert.Equal(MinesweeperActionResult.Accepted, game.Reveal(new CellId(0, 0)));
        Assert.NotEqual(MinesweeperStatus.Lost, game.Status);
        Assert.True(game.GetCell(new CellId(0, 0)).IsRevealed);
        Assert.Equal(1, game.MineCount);
    }

    [Fact]
    public void ZeroRevealFloodsConnectedSafeCellsButStopsAtNumbers()
    {
        var game = new MinesweeperGame(4, 3, new[] { new CellId(0, 3) });
        game.Reveal(new CellId(2, 0));
        Assert.True(game.GetCell(new CellId(2, 0)).IsRevealed);
        Assert.True(game.GetCell(new CellId(1, 2)).IsRevealed);
        Assert.False(game.GetCell(new CellId(0, 3)).IsRevealed);
    }

    [Fact]
    public void FlagsPreventRevealAndCanBeToggledOff()
    {
        var game = new MinesweeperGame(2, 2, new[] { new CellId(0, 0) });
        var cell = new CellId(1, 1);
        Assert.Equal(MinesweeperActionResult.Accepted, game.ToggleFlag(cell));
        Assert.Equal(MinesweeperActionResult.AlreadyFlagged, game.Reveal(cell));
        Assert.Equal(1, game.FlagCount);
        Assert.Equal(MinesweeperActionResult.Accepted, game.ToggleFlag(cell));
        Assert.Equal(0, game.FlagCount);
    }

    [Fact]
    public void RevealingMineLosesAndTerminalActionsAreRejected()
    {
        // Leave the mine accessible after the first safe reveal. A 1D board would
        // flood-reveal every safe cell and win before the loss transition is tested.
        var game = new MinesweeperGame(4, 4, new[] { new CellId(0, 0) });
        // Reveal a numbered safe cell adjacent to the mine so flood reveal
        // cannot finish the board before the loss transition is exercised.
        game.Reveal(new CellId(0, 1));
        Assert.Equal(MinesweeperActionResult.Accepted, game.Reveal(new CellId(0, 0)));
        Assert.Equal(MinesweeperStatus.Lost, game.Status);
        Assert.Equal(MinesweeperActionResult.RejectedTerminal, game.ToggleFlag(new CellId(0, 1)));
    }

    [Fact]
    public void RevealingAllSafeCellsWins()
    {
        var game = new MinesweeperGame(2, 2, new[] { new CellId(0, 0) });
        game.Reveal(new CellId(1, 1));
        game.Reveal(new CellId(0, 1));
        game.Reveal(new CellId(1, 0));
        Assert.Equal(MinesweeperStatus.Won, game.Status);
        Assert.Equal(0, game.RemainingSafeCount);
    }

    [Fact]
    public void ResetRestoresReadyStateAndActions()
    {
        var game = new MinesweeperGame(3, 3, new[] { new CellId(0, 0) });
        game.ToggleFlag(new CellId(1, 1));
        game.Reveal(new CellId(2, 2));
        game.Reset();
        Assert.Equal(MinesweeperStatus.Ready, game.Status);
        Assert.Equal(0, game.FlagCount);
        Assert.Equal(0, game.RevealedSafeCount);
        Assert.Equal(MinesweeperActionResult.Accepted, game.Reveal(new CellId(2, 2)));
    }

    [Fact]
    public void ResetRestoresOriginalFixedMineLayoutAfterFirstRevealRelocation()
    {
        var game = new MinesweeperGame(4, 4, new[] { new CellId(0, 0) });
        game.Reveal(new CellId(0, 0));
        game.Reset();

        // A numbered first reveal leaves the original mine accessible.
        game.Reveal(new CellId(0, 1));
        Assert.Equal(MinesweeperActionResult.Accepted, game.Reveal(new CellId(0, 0)));
        Assert.Equal(MinesweeperStatus.Lost, game.Status);
    }

    [Fact]
    public void InvalidCoordinatesAreRejectedWithoutMutation()
    {
        var game = new MinesweeperGame(2, 2, new[] { new CellId(0, 0) });
        Assert.Equal(MinesweeperActionResult.RejectedOutOfBounds, game.Reveal(new CellId(-1, 0)));
        Assert.Equal(0, game.RevealedSafeCount);
        Assert.Throws<ArgumentOutOfRangeException>(() => game.GetCell(new CellId(2, 0)));
    }

    [Fact]
    public void RepeatedRevealIsExplicitlyRejected()
    {
        var game = new MinesweeperGame(2, 2, new[] { new CellId(0, 0) });
        game.Reveal(new CellId(1, 1));
        Assert.Equal(MinesweeperActionResult.AlreadyRevealed, game.Reveal(new CellId(1, 1)));
    }
}
