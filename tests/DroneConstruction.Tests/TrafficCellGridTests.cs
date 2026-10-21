using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class TrafficCellGridTests
{
    [Fact]
    public void AgentCannotOccupyTwoCellsAtOnce()
    {
        var grid = new TrafficCellGrid(3, 1);
        var origin = new GridCell(0, 0);
        var destination = new GridCell(1, 0);

        Assert.True(grid.TryOccupy(origin, 7));
        Assert.False(grid.TryOccupy(destination, 7));
        Assert.True(grid.TryGetOccupant(origin, out var owner));
        Assert.Equal((ulong)7, owner);
        Assert.False(grid.IsOccupied(destination));
    }

    [Fact]
    public void ReleaseRequiresCorrectOwnerAndAllowsSubsequentMove()
    {
        var grid = new TrafficCellGrid(3, 1);
        var origin = new GridCell(0, 0);
        var destination = new GridCell(1, 0);

        Assert.True(grid.TryOccupy(origin, 7));
        Assert.False(grid.Release(origin, 8));
        Assert.True(grid.IsOccupied(origin));
        Assert.True(grid.Release(origin, 7));
        Assert.True(grid.TryOccupy(destination, 7));
        Assert.True(grid.TryGetOccupant(destination, out var owner));
        Assert.Equal((ulong)7, owner);
    }

    [Fact]
    public void BlockedMovePreservesBothOccupanciesAndAgentIndex()
    {
        var grid = new TrafficCellGrid(3, 1);
        var origin = new GridCell(0, 0);
        var blocked = new GridCell(1, 0);
        var other = new GridCell(2, 0);

        Assert.True(grid.TryOccupy(origin, 7));
        Assert.True(grid.TryOccupy(blocked, 8));

        Assert.False(grid.TryMove(origin, blocked, 7));

        Assert.True(grid.TryGetOccupant(origin, out var originOwner));
        Assert.Equal((ulong)7, originOwner);
        Assert.True(grid.TryGetOccupant(blocked, out var blockedOwner));
        Assert.Equal((ulong)8, blockedOwner);
        Assert.False(grid.TryMove(other, new GridCell(1, 0), 7));
        Assert.True(grid.TryMove(origin, other, 7));
        Assert.False(grid.IsOccupied(origin));
        Assert.True(grid.TryGetOccupant(other, out var movedOwner));
        Assert.Equal((ulong)7, movedOwner);
    }

    [Fact]
    public void MoveRequiresCorrectOwnerAndValidCells()
    {
        var grid = new TrafficCellGrid(2, 1);
        var origin = new GridCell(0, 0);
        var destination = new GridCell(1, 0);
        Assert.True(grid.TryOccupy(origin, 7));

        Assert.False(grid.TryMove(origin, destination, 8));
        Assert.True(grid.IsOccupied(origin));
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.TryMove(origin, new GridCell(2, 0), 7));
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.TryMove(origin, destination, 0));
        Assert.True(grid.TryMove(origin, origin, 7));
        Assert.True(grid.IsOccupied(origin));
    }

    [Fact]
    public void TransferMovesOwnershipBetweenGridsWithoutDuplicatingAgent()
    {
        var source = new TrafficCellGrid(2, 1);
        var destination = new TrafficCellGrid(2, 1);
        var origin = new GridCell(0, 0);
        var target = new GridCell(1, 0);
        Assert.True(source.TryOccupy(origin, 7));

        Assert.True(source.TryTransferTo(origin, target, 7, destination));

        Assert.False(source.IsOccupied(origin));
        Assert.True(destination.TryGetOccupant(target, out var owner));
        Assert.Equal((ulong)7, owner);
        Assert.True(source.TryOccupy(new GridCell(1, 0), 7)); // Source no longer owns this agent.
    }

    [Fact]
    public void BlockedTransferPreservesSourceOccupancy()
    {
        var source = new TrafficCellGrid(2, 1);
        var destination = new TrafficCellGrid(2, 1);
        var origin = new GridCell(0, 0);
        var target = new GridCell(1, 0);
        Assert.True(source.TryOccupy(origin, 7));
        Assert.True(destination.TryOccupy(target, 8));

        Assert.False(source.TryTransferTo(origin, target, 7, destination));

        Assert.True(source.TryGetOccupant(origin, out var sourceOwner));
        Assert.Equal((ulong)7, sourceOwner);
        Assert.True(destination.TryGetOccupant(target, out var destinationOwner));
        Assert.Equal((ulong)8, destinationOwner);
    }

    [Fact]
    public void TransferWithinSameGridUsesAtomicMoveSemantics()
    {
        var grid = new TrafficCellGrid(3, 1);
        var origin = new GridCell(0, 0);
        var target = new GridCell(1, 0);
        Assert.True(grid.TryOccupy(origin, 7));

        Assert.True(grid.TryTransferTo(origin, target, 7, grid));

        Assert.False(grid.IsOccupied(origin));
        Assert.True(grid.TryGetOccupant(target, out var owner));
        Assert.Equal((ulong)7, owner);
    }

    [Fact]
    public void TransferFromWrongSourceOwnerDoesNotModifyEitherGrid()
    {
        var source = new TrafficCellGrid(2, 1);
        var destination = new TrafficCellGrid(2, 1);
        var origin = new GridCell(0, 0);
        var target = new GridCell(1, 0);
        Assert.True(source.TryOccupy(origin, 7));

        Assert.False(source.TryTransferTo(origin, target, 8, destination));

        Assert.True(source.TryGetOccupant(origin, out var owner));
        Assert.Equal((ulong)7, owner);
        Assert.False(destination.IsOccupied(target));
    }

    [Fact]
    public void TransferRejectsDestinationAgentAlreadyPresentInTargetGrid()
    {
        var source = new TrafficCellGrid(2, 1);
        var destination = new TrafficCellGrid(2, 1);
        var origin = new GridCell(0, 0);
        var target = new GridCell(1, 0);
        Assert.True(source.TryOccupy(origin, 7));
        Assert.True(destination.TryOccupy(new GridCell(0, 0), 7));

        Assert.False(source.TryTransferTo(origin, target, 7, destination));

        Assert.True(source.TryGetOccupant(origin, out var sourceOwner));
        Assert.Equal((ulong)7, sourceOwner);
        Assert.True(destination.TryGetOccupant(new GridCell(0, 0), out var destinationOwner));
        Assert.Equal((ulong)7, destinationOwner);
        Assert.False(destination.IsOccupied(target));
    }

    [Fact]
    public void TransferRejectsOutOfBoundsTargetBeforeReleasingSource()
    {
        var source = new TrafficCellGrid(2, 1);
        var destination = new TrafficCellGrid(1, 1);
        var origin = new GridCell(0, 0);
        Assert.True(source.TryOccupy(origin, 7));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            source.TryTransferTo(origin, new GridCell(1, 0), 7, destination));

        Assert.True(source.TryGetOccupant(origin, out var sourceOwner));
        Assert.Equal((ulong)7, sourceOwner);
        Assert.False(destination.IsOccupied(new GridCell(0, 0))); // No transfer side effect on the destination grid.
    }

    [Fact]
    public void RejectsReservedAgentIdZero()
    {
        var grid = new TrafficCellGrid(1, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.TryOccupy(new GridCell(0, 0), 0));
    }
}
