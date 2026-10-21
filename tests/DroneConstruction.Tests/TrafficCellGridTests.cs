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
    public void RejectsReservedAgentIdZero()
    {
        var grid = new TrafficCellGrid(1, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => grid.TryOccupy(new GridCell(0, 0), 0));
    }
}
