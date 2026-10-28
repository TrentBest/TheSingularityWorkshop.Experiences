using TheSingularityWorkshop.Experiences.CapsuleClinic;
using Xunit;

namespace TheSingularityWorkshop.Experiences.CapsuleClinic.Tests;

public sealed class CapsuleVirusBloomTests
{
    [Fact]
    public void BloomSpreadsOneSnapshotWaveAtATimeAcrossTheBoard()
    {
        var board = new CapsuleBoard(width: 5, height: 5);
        Assert.True(board.TryPlaceVirus(2, 2, CapsuleColor.Purple));

        var result = board.ApplyVirusBloom();

        Assert.Equal(4, result.Waves);
        Assert.Equal(24, result.CellsInfected);
        Assert.Equal(25, board.OccupiedCount);
        Assert.All(board.Snapshot(), cell =>
        {
            Assert.True(cell.HasValue);
            Assert.True(cell!.Value.IsVirus);
            Assert.Equal(CapsuleColor.Purple, cell.Value.Color);
        });
    }

    [Fact]
    public void BloomWithNoVirusSeedsDoesNotInventAVirus()
    {
        var board = new CapsuleBoard(width: 4, height: 3);

        var result = board.ApplyVirusBloom();

        Assert.Equal(new VirusBloomResult(0, 0), result);
        Assert.Equal(0, board.OccupiedCount);
    }

    [Fact]
    public void BloomDoesNotCrossCapsuleBarrierIntoEnclosedEmptyCell()
    {
        var board = new CapsuleBoard(width: 5, height: 5);
        Assert.True(board.TryPlaceVirus(0, 0, CapsuleColor.Red));
        Assert.True(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Blue, CapsuleColor.Blue), 0, 2, CapsuleOrientation.Horizontal));
        Assert.True(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Yellow, CapsuleColor.Yellow), 3, 2, CapsuleOrientation.Horizontal));
        Assert.True(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Purple, CapsuleColor.Purple), 2, 0, CapsuleOrientation.Vertical));
        Assert.True(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Blue, CapsuleColor.Blue), 2, 3, CapsuleOrientation.Vertical));

        var result = board.ApplyVirusBloom();

        Assert.True(result.CellsInfected > 0);
        Assert.Null(board.GetCell(2, 2));
        Assert.True(board.GetCell(0, 2) is { IsVirus: false });
        Assert.True(board.GetCell(2, 0) is { IsVirus: false });
    }

    [Fact]
    public void BloomDoesNotInfectAnEmptyCellReachableOnlyDiagonally()
    {
        var board = new CapsuleBoard(width: 3, height: 3);
        Assert.True(board.TryPlaceVirus(0, 0, CapsuleColor.Yellow));
        Assert.True(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Red, CapsuleColor.Blue), 1, 0, CapsuleOrientation.Horizontal));
        Assert.True(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Purple, CapsuleColor.Purple), 0, 1, CapsuleOrientation.Vertical));

        var result = board.ApplyVirusBloom();

        Assert.Equal(new VirusBloomResult(0, 0), result);
        Assert.Null(board.GetCell(1, 1));
        Assert.True(board.GetCell(1, 0) is { IsVirus: false });
        Assert.True(board.GetCell(0, 1) is { IsVirus: false });
    }

    [Fact]
    public void BloomIsDeterministicForEquivalentStartingBoards()
    {
        var first = CreateSeededBoard();
        var second = CreateSeededBoard();

        var firstResult = first.ApplyVirusBloom();
        var secondResult = second.ApplyVirusBloom();

        Assert.Equal(firstResult, secondResult);
        Assert.Equal(first.Snapshot(), second.Snapshot());
    }

    [Fact]
    public void BloomStopsWhenTheBoardBecomesFull()
    {
        var board = new CapsuleBoard(width: 2, height: 2);
        Assert.True(board.TryPlaceVirus(0, 0, CapsuleColor.Yellow));
        Assert.True(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Red, CapsuleColor.Blue), 1, 0, CapsuleOrientation.Vertical));

        var result = board.ApplyVirusBloom();

        Assert.Equal(new VirusBloomResult(1, 1), result);
        Assert.Equal(4, board.OccupiedCount);
        Assert.True(board.GetCell(0, 1) is { IsVirus: true });
    }

    private static CapsuleBoard CreateSeededBoard()
    {
        var board = new CapsuleBoard(width: 6, height: 5);
        Assert.True(board.TryPlaceVirus(0, 0, CapsuleColor.Purple));
        Assert.True(board.TryPlaceVirus(5, 4, CapsuleColor.Yellow));
        Assert.True(board.TryPlaceCapsule(new CapsulePair(CapsuleColor.Red, CapsuleColor.Blue), 2, 1, CapsuleOrientation.Horizontal));
        return board;
    }
}
