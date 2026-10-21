using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class SkylanePolicyTests
{
    [Theory]
    [InlineData(LaneDirection.East, 0, 0, 9, 0, 3, 3, 0)]
    [InlineData(LaneDirection.West, 9, 0, 0, 0, 2, 7, 0)]
    [InlineData(LaneDirection.North, 0, 9, 0, 0, 4, 0, 5)]
    [InlineData(LaneDirection.South, 0, 0, 0, 9, 3, 0, 3)]
    public void NextCellFollowsDeclaredDirectionAndSpeed(
        LaneDirection direction,
        int currentX,
        int currentY,
        int targetX,
        int targetY,
        int cellsPerTick,
        int expectedX,
        int expectedY)
    {
        var lane = new Skylane("lane", 10, 10, direction, elevation: 2, cellsPerTick: cellsPerTick);
        Assert.True(lane.TryGetNextCell(
            new GridCell(currentX, currentY),
            new GridCell(targetX, targetY),
            out var next));
        Assert.Equal(new GridCell(expectedX, expectedY), next);
    }

    [Theory]
    [InlineData(LaneDirection.East, 4, 4, 3, 4)]
    [InlineData(LaneDirection.West, 4, 4, 5, 4)]
    [InlineData(LaneDirection.North, 4, 4, 4, 5)]
    [InlineData(LaneDirection.South, 4, 4, 4, 3)]
    public void NextCellRejectsReverseTravel(
        LaneDirection direction, int x, int y, int targetX, int targetY)
    {
        var lane = new Skylane("lane", 10, 10, direction, elevation: 0);
        Assert.False(lane.TryGetNextCell(new GridCell(x, y), new GridCell(targetX, targetY), out var next));
        Assert.Equal(new GridCell(x, y), next);
    }

    [Theory]
    [InlineData(4, 4, 5, 5)]
    [InlineData(4, 4, 4, 4)]
    public void NextCellRejectsDiagonalOrAlreadyReachedTarget(int x, int y, int targetX, int targetY)
    {
        var lane = new Skylane("lane", 10, 10, LaneDirection.East, elevation: 0);
        var current = new GridCell(x, y);

        Assert.False(lane.TryGetNextCell(current, new GridCell(targetX, targetY), out var next));
        Assert.Equal(current, next);
    }

    [Fact]
    public void NextCellRejectsTargetsOutsideLane()
    {
        var lane = new Skylane("lane", 3, 3, LaneDirection.East, elevation: 0);
        Assert.False(lane.TryGetNextCell(new GridCell(0, 0), new GridCell(3, 0), out _));
    }
}
