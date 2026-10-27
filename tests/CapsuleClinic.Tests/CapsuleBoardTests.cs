using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class CapsuleBoardTests
{
    [Fact]
    public void PlaceCapsule_IsAtomicWhenPartnerWouldBeOutOfBounds()
    {
        var board = new CapsuleBoard(width: 4, height: 4);

        Assert.False(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Red, CapsuleColor.Blue), 3, 0, CapsuleOrientation.Horizontal));
        Assert.Equal(0, board.OccupiedCount);
    }

    [Fact]
    public void PlaceCapsule_DoesNotOverwriteOccupiedCells()
    {
        var board = new CapsuleBoard(width: 4, height: 4);
        Assert.True(board.TryPlaceVirus(1, 1, CapsuleColor.Yellow));

        Assert.False(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Red, CapsuleColor.Blue), 0, 1, CapsuleOrientation.Horizontal));
        Assert.Null(board.GetCell(0, 1));
        Assert.True(board.GetCell(1, 1)!.Value.IsVirus);
        Assert.Equal(1, board.OccupiedCount);
    }

    [Fact]
    public void Resolve_ClearsHorizontalRunAndCountsVirusTargets()
    {
        var board = new CapsuleBoard(width: 4, height: 4);
        Assert.True(board.TryPlaceVirus(0, 0, CapsuleColor.Red));
        Assert.True(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Red, CapsuleColor.Red), 1, 0, CapsuleOrientation.Horizontal));

        var result = board.ResolveMatchesAndGravity();

        Assert.Equal(new ResolutionResult(1, 3, 1), result);
        Assert.Equal(0, board.OccupiedCount);
    }

    [Fact]
    public void Resolve_ClearsVerticalRun()
    {
        var board = new CapsuleBoard(width: 4, height: 5);
        Assert.True(board.TryPlaceVirus(0, 0, CapsuleColor.Blue));
        Assert.True(board.TryPlaceVirus(0, 1, CapsuleColor.Blue));
        Assert.True(board.TryPlaceVirus(0, 2, CapsuleColor.Blue));
        Assert.True(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Blue, CapsuleColor.Yellow), 0, 3, CapsuleOrientation.Vertical));

        var result = board.ResolveMatchesAndGravity();

        Assert.Equal(1, result.ChainCount);
        Assert.Equal(4, result.CellsCleared);
        Assert.Equal(3, result.VirusesCleared);
        Assert.Equal(new PuzzleCell(CapsuleColor.Yellow, false), board.GetCell(0, 4));
    }

    [Fact]
    public void GravityCompactsIndependentSegmentsAfterMatch()
    {
        var board = new CapsuleBoard(width: 5, height: 6);
        Assert.True(board.TryPlaceVirus(0, 0, CapsuleColor.Red));
        Assert.True(board.TryPlaceVirus(1, 0, CapsuleColor.Red));
        Assert.True(board.TryPlaceVirus(2, 0, CapsuleColor.Red));
        Assert.True(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Red, CapsuleColor.Blue), 3, 0, CapsuleOrientation.Horizontal));
        Assert.True(board.TryPlaceVirus(0, 2, CapsuleColor.Yellow));

        board.ResolveMatchesAndGravity();

        Assert.Equal(new PuzzleCell(CapsuleColor.Yellow, true), board.GetCell(0, 0));
    }

    [Fact]
    public void Snapshot_IsIndependentAndBottomRowFirst()
    {
        var board = new CapsuleBoard(width: 4, height: 4);
        Assert.True(board.TryPlaceVirus(1, 0, CapsuleColor.Purple));
        var snapshot = board.Snapshot();

        Assert.True(board.TryPlaceVirus(1, 1, CapsuleColor.Blue));
        Assert.Equal(16, snapshot.Count);
        Assert.Equal(new PuzzleCell(CapsuleColor.Purple, true), snapshot[1]);
        Assert.Null(snapshot[5]);
    }

    [Fact]
    public void ConstructorAndCoordinatesRejectInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CapsuleBoard(0, 4));
        var board = new CapsuleBoard();
        Assert.Throws<ArgumentOutOfRangeException>(() => board.GetCell(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => board.TryPlaceVirus(0, 0, (CapsuleColor)99));
    }
}
