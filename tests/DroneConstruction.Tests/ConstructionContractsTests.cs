using TheSingularityWorkshop.Experiences.DroneConstruction;
using Xunit;

namespace TheSingularityWorkshop.Experiences.DroneConstruction.Tests;

public sealed class ConstructionContractsTests
{
    [Fact]
    public void HeightField_MapsRgbChannelToBoundedIntegerHeight()
    {
        var field = ImageHeightField.FromRgb(2, 1, [0, 127, 255, 255, 10, 0], 10,
            ChannelHeightMapping.Red, "sha256:example");

        Assert.Equal(0, field[0, 0]);
        Assert.Equal(10, field[1, 0]);
        Assert.Equal(10, field.MaximumHeight);
    }

    [Fact]
    public void HeightField_RejectsWrongPixelBufferLengthAndInvalidCoordinates()
    {
        Assert.Throws<ArgumentException>(() =>
            ImageHeightField.FromRgb(2, 1, [1, 2, 3], 10, ChannelHeightMapping.Red, "digest"));

        var field = ImageHeightField.FromRgb(1, 1, [0, 0, 0], 5, ChannelHeightMapping.Average, "digest");
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = field[1, 0]);
    }

    [Fact]
    public void TrafficGrid_EnforcesExclusiveOccupancyAndOwnerRelease()
    {
        var grid = new TrafficCellGrid(3, 2);
        var cell = new GridCell(1, 1);

        Assert.True(grid.TryOccupy(cell, 7));
        Assert.False(grid.TryOccupy(cell, 8));
        Assert.False(grid.Release(cell, 8));
        Assert.True(grid.IsOccupied(cell));
        Assert.True(grid.Release(cell, 7));
        Assert.False(grid.IsOccupied(cell));
    }

    [Fact]
    public void TrafficGrid_DoesNotAllowOneAgentInTwoCells()
    {
        var grid = new TrafficCellGrid(3, 2);
        Assert.True(grid.TryOccupy(new GridCell(0, 0), 7));
        Assert.False(grid.TryOccupy(new GridCell(1, 0), 7));
    }

    [Fact]
    public void Slipstream_RequiresClearDestinationAndExactlyConfiguredLookahead()
    {
        var lane = new TrafficCellGrid(10, 2);
        var slipstream = new Slipstream("transition-a", transitionCellCount: 2, lookaheadCells: 5);
        var destination = new GridCell(8, 0);
        GridCell[] lookahead =
        [
            new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)
        ];

        Assert.True(slipstream.TryReserve(7, destination, lookahead, lane));
        Assert.Equal(7UL, slipstream.ReservedBy);
        Assert.False(slipstream.TryReserve(8, destination, lookahead, lane));
        Assert.True(slipstream.Release(7));
        Assert.Null(slipstream.ReservedBy);
    }

    [Fact]
    public void Slipstream_DoesNotAllowOwnerToReplaceActiveReservation()
    {
        var lane = new TrafficCellGrid(12, 1);
        var slipstream = new Slipstream("transition-a", transitionCellCount: 2, lookaheadCells: 5);
        GridCell[] firstLookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];
        GridCell[] secondLookahead = [new(3, 0), new(4, 0), new(5, 0), new(6, 0), new(7, 0)];
        var firstDestination = new GridCell(8, 0);
        var secondDestination = new GridCell(9, 0);

        Assert.True(slipstream.TryReserve(7, firstDestination, firstLookahead, lane));

        Assert.False(slipstream.TryReserve(7, secondDestination, secondLookahead, lane));
        Assert.Equal(7UL, slipstream.ReservedBy);
        Assert.True(slipstream.Release(7));
        Assert.True(slipstream.TryReserve(8, secondDestination, secondLookahead, lane));
    }

    [Fact]
    public void Slipstream_RejectsReleaseByNonOwnerAndPreservesReservation()
    {
        var lane = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("transition-a", transitionCellCount: 2, lookaheadCells: 5);
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];

        Assert.True(slipstream.TryReserve(7, new GridCell(8, 0), lookahead, lane));

        Assert.False(slipstream.Release(8));
        Assert.Equal(7UL, slipstream.ReservedBy);
        Assert.False(slipstream.TryReserve(8, new GridCell(9, 0), lookahead, lane));

        Assert.True(slipstream.Release(7));
        Assert.Null(slipstream.ReservedBy);
    }

    [Fact]
    public void Slipstream_RejectsOccupiedLookaheadAndWrongLookaheadLength()
    {
        var lane = new TrafficCellGrid(10, 2);
        var slipstream = new Slipstream("transition-a", transitionCellCount: 2, lookaheadCells: 5);
        GridCell[] lookahead =
        [
            new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)
        ];

        Assert.True(lane.TryOccupy(new GridCell(4, 0), 99));
        Assert.False(slipstream.TryReserve(7, new GridCell(8, 0), lookahead, lane));
        Assert.Throws<ArgumentException>(() => slipstream.TryReserve(7, new GridCell(8, 0), lookahead[..4], lane));
    }

    [Fact]
    public void Slipstream_RejectsDuplicateLookaheadCellsAndDestinationOverlap()
    {
        var lane = new TrafficCellGrid(10, 2);
        var slipstream = new Slipstream("transition-a", transitionCellCount: 2, lookaheadCells: 5);
        GridCell[] duplicateLookahead =
        [
            new(2, 0), new(3, 0), new(3, 0), new(5, 0), new(6, 0)
        ];
        GridCell[] destinationOverlap =
        [
            new(2, 0), new(3, 0), new(8, 0), new(5, 0), new(6, 0)
        ];

        Assert.False(slipstream.TryReserve(7, new GridCell(8, 0), duplicateLookahead, lane));
        Assert.False(slipstream.TryReserve(7, new GridCell(8, 0), destinationOverlap, lane));
        Assert.Null(slipstream.ReservedBy);
    }

    [Fact]
    public void Slipstream_RejectsOutOfBoundsLookaheadWithoutThrowing()
    {
        var lane = new TrafficCellGrid(10, 2);
        var slipstream = new Slipstream("transition-b", transitionCellCount: 2, lookaheadCells: 5);
        GridCell[] lookahead =
        [
            new(2, 0), new(3, 0), new(10, 0), new(5, 0), new(6, 0)
        ];

        Assert.False(slipstream.TryReserve(8, new GridCell(8, 0), lookahead, lane));
        Assert.Null(slipstream.ReservedBy);
    }

    [Fact]
    public void SlipstreamAdmission_TransfersOccupancyAndRetainsReservationUntilReleased()
    {
        var source = new TrafficCellGrid(2, 1);
        var lane = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("transition-a", transitionCellCount: 2, lookaheadCells: 5);
        var origin = new GridCell(0, 0);
        var destination = new GridCell(8, 0);
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];
        Assert.True(source.TryOccupy(origin, 7));

        Assert.True(slipstream.TryAdmit(7, source, origin, destination, lookahead, lane));

        Assert.False(source.IsOccupied(origin));
        Assert.True(lane.TryGetOccupant(destination, out var occupant));
        Assert.Equal(7UL, occupant);
        Assert.Equal(7UL, slipstream.ReservedBy);

        // A second admission attempt cannot undo the active reservation after the
        // first transfer has already removed the agent from the source grid.
        Assert.False(slipstream.TryAdmit(7, source, origin, destination, lookahead, lane));
        Assert.Equal(7UL, slipstream.ReservedBy);

        Assert.True(slipstream.Release(7));
        Assert.Null(slipstream.ReservedBy);

        // Releasing transition capacity must not erase the admitted agent's lane occupancy.
        Assert.True(lane.TryGetOccupant(destination, out occupant));
        Assert.Equal(7UL, occupant);
    }

    [Fact]
    public void SlipstreamAdmission_RejectsUsingOneGridAsBothSourceAndDestination()
    {
        var grid = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("transition-same-grid", transitionCellCount: 2, lookaheadCells: 5);
        var origin = new GridCell(0, 0);
        var destination = new GridCell(8, 0);
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];
        Assert.True(grid.TryOccupy(origin, 7));

        Assert.False(slipstream.TryAdmit(7, grid, origin, destination, lookahead, grid));

        Assert.True(grid.TryGetAgentCell(7, out var indexedCell));
        Assert.Equal(origin, indexedCell);
        Assert.True(grid.TryGetOccupant(origin, out var owner));
        Assert.Equal(7UL, owner);
        Assert.False(grid.IsOccupied(destination));
        Assert.Null(slipstream.ReservedBy);
    }

    [Fact]
    public void SlipstreamAdmission_LeavesSourceOccupiedWhenDestinationIsBlocked()
    {
        var source = new TrafficCellGrid(2, 1);
        var lane = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("transition-a", transitionCellCount: 2, lookaheadCells: 5);
        var origin = new GridCell(0, 0);
        var destination = new GridCell(8, 0);
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];
        Assert.True(source.TryOccupy(origin, 7));
        Assert.True(lane.TryOccupy(destination, 99));

        Assert.False(slipstream.TryAdmit(7, source, origin, destination, lookahead, lane));

        Assert.True(source.TryGetOccupant(origin, out var sourceOwner));
        Assert.Equal(7UL, sourceOwner);
        Assert.True(lane.TryGetOccupant(destination, out var destinationOwner));
        Assert.Equal(99UL, destinationOwner);
        Assert.Null(slipstream.ReservedBy);
    }

    [Fact]
    public void SlipstreamAdmission_RollsBackReservationWhenSourceTransferFails()
    {
        var source = new TrafficCellGrid(2, 1);
        var lane = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("transition-a", transitionCellCount: 2, lookaheadCells: 5);
        var origin = new GridCell(0, 0);
        var destination = new GridCell(8, 0);
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];
        Assert.True(source.TryOccupy(origin, 99));

        Assert.False(slipstream.TryAdmit(7, source, origin, destination, lookahead, lane));

        Assert.True(source.TryGetOccupant(origin, out var sourceOwner));
        Assert.Equal(99UL, sourceOwner);
        Assert.False(lane.IsOccupied(destination));
        Assert.Null(slipstream.ReservedBy);
    }

    [Fact]
    public void SlipstreamAdmission_RejectsAgentAlreadyRegisteredInDestinationGrid()
    {
        var source = new TrafficCellGrid(2, 1);
        var lane = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("transition-a", transitionCellCount: 2, lookaheadCells: 5);
        var origin = new GridCell(0, 0);
        var destination = new GridCell(8, 0);
        var existingLaneCell = new GridCell(1, 0);
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];
        Assert.True(source.TryOccupy(origin, 7));
        Assert.True(lane.TryOccupy(existingLaneCell, 7));

        Assert.False(slipstream.TryAdmit(7, source, origin, destination, lookahead, lane));

        Assert.True(source.TryGetOccupant(origin, out var sourceOwner));
        Assert.Equal(7UL, sourceOwner);
        Assert.True(lane.TryGetOccupant(existingLaneCell, out var laneOwner));
        Assert.Equal(7UL, laneOwner);
        Assert.False(lane.IsOccupied(destination));
        Assert.Null(slipstream.ReservedBy);
    }

    [Fact]
    public void SlipstreamAdmission_ReleasesReservationWhenSourceCoordinateThrows()
    {
        var source = new TrafficCellGrid(2, 1);
        var lane = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("transition-a", transitionCellCount: 2, lookaheadCells: 5);
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            slipstream.TryAdmit(7, source, new GridCell(2, 0), new GridCell(8, 0), lookahead, lane));

        Assert.Null(slipstream.ReservedBy);
        Assert.False(lane.IsOccupied(new GridCell(8, 0)));
    }

    [Fact]
    public void SlipstreamLifecycle_TransfersThroughTransitionAndReleasesAfterExit()
    {
        var source = new TrafficCellGrid(2, 1);
        var transition = new TrafficCellGrid(4, 1);
        var destination = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("lifecycle-a", transitionCellCount: 4, lookaheadCells: 5);
        var lifecycle = new SlipstreamLifecycle(slipstream, transition);
        var origin = new GridCell(0, 0);
        var entry = new GridCell(0, 0);
        var destinationCell = new GridCell(8, 0);
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];
        Assert.True(source.TryOccupy(origin, 7));

        Assert.True(lifecycle.TryEnter(7, source, origin, entry, destination, destinationCell, lookahead));
        Assert.False(source.TryGetAgentCell(7, out _));
        Assert.True(transition.TryGetAgentCell(7, out var enteredCell));
        Assert.Equal(entry, enteredCell);
        Assert.Equal(7UL, lifecycle.AgentId);

        Assert.False(lifecycle.TryAdvance(7, new GridCell(2, 0)));
        Assert.True(transition.TryOccupy(new GridCell(2, 0), 99));
        Assert.True(lifecycle.TryAdvance(7, new GridCell(1, 0)));
        Assert.False(lifecycle.TryAdvance(7, new GridCell(2, 0)));
        Assert.Equal(7UL, lifecycle.AgentId);
        Assert.True(transition.Release(new GridCell(2, 0), 99));
        Assert.True(lifecycle.TryAdvance(7, new GridCell(2, 0)));
        Assert.True(lifecycle.TryAdvance(7, new GridCell(3, 0)));

        Assert.True(lifecycle.TryExit(7, destination, destinationCell));
        Assert.False(transition.TryGetAgentCell(7, out _));
        Assert.True(destination.TryGetAgentCell(7, out var exitedCell));
        Assert.Equal(destinationCell, exitedCell);
        Assert.Null(lifecycle.AgentId);
    }

    [Fact]
    public void SlipstreamLifecycle_BlockedExitRetainsTransitionOccupancyAndReservation()
    {
        var source = new TrafficCellGrid(2, 1);
        var transition = new TrafficCellGrid(2, 1);
        var destination = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("lifecycle-b", transitionCellCount: 2, lookaheadCells: 5);
        var lifecycle = new SlipstreamLifecycle(slipstream, transition);
        var origin = new GridCell(0, 0);
        var entry = new GridCell(0, 0);
        var destinationCell = new GridCell(8, 0);
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];
        Assert.True(source.TryOccupy(origin, 7));
        Assert.True(lifecycle.TryEnter(7, source, origin, entry, destination, destinationCell, lookahead));
        Assert.True(destination.TryOccupy(destinationCell, 99));

        Assert.False(lifecycle.TryExit(7, destination, destinationCell));

        Assert.True(transition.TryGetAgentCell(7, out var transitionCell));
        Assert.Equal(entry, transitionCell);
        Assert.Equal(7UL, lifecycle.AgentId);
        Assert.True(destination.TryGetOccupant(destinationCell, out var occupant));
        Assert.Equal(99UL, occupant);
    }

    [Fact]
    public void SlipstreamLifecycle_RejectsExitToUnreservedLaneOrCell()
    {
        var source = new TrafficCellGrid(2, 1);
        var transition = new TrafficCellGrid(2, 1);
        var reservedLane = new TrafficCellGrid(10, 1);
        var otherLane = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("lifecycle-bound-exit", transitionCellCount: 2, lookaheadCells: 5);
        var lifecycle = new SlipstreamLifecycle(slipstream, transition);
        var origin = new GridCell(0, 0);
        var entry = new GridCell(0, 0);
        var destination = new GridCell(8, 0);
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];
        Assert.True(source.TryOccupy(origin, 7));
        Assert.True(lifecycle.TryEnter(7, source, origin, entry, reservedLane, destination, lookahead));

        Assert.False(lifecycle.TryExit(7, otherLane, destination));
        Assert.False(lifecycle.TryExit(7, reservedLane, new GridCell(9, 0)));
        Assert.True(transition.TryGetAgentCell(7, out var currentCell));
        Assert.Equal(entry, currentCell);
        Assert.Equal(7UL, lifecycle.AgentId);
        Assert.False(otherLane.IsOccupied(destination));
        Assert.False(reservedLane.IsOccupied(destination));

        Assert.True(lifecycle.TryExit(7, reservedLane, destination));
        Assert.Null(lifecycle.AgentId);
    }

    [Fact]
    public void SlipstreamLifecycle_FailedEntryPreservesSourceAndDoesNotKeepReservation()
    {
        var source = new TrafficCellGrid(2, 1);
        var transition = new TrafficCellGrid(1, 1);
        var destination = new TrafficCellGrid(10, 1);
        var slipstream = new Slipstream("lifecycle-c", transitionCellCount: 1, lookaheadCells: 5);
        var lifecycle = new SlipstreamLifecycle(slipstream, transition);
        var origin = new GridCell(0, 0);
        var entry = new GridCell(0, 0);
        var destinationCell = new GridCell(8, 0);
        GridCell[] lookahead = [new(2, 0), new(3, 0), new(4, 0), new(5, 0), new(6, 0)];
        Assert.True(source.TryOccupy(origin, 99));

        Assert.False(lifecycle.TryEnter(7, source, origin, entry, destination, destinationCell, lookahead));

        Assert.True(source.TryGetAgentCell(99, out var sourceCell));
        Assert.Equal(origin, sourceCell);
        Assert.False(transition.IsOccupied(entry));
        Assert.Null(slipstream.ReservedBy);
        Assert.Null(lifecycle.AgentId);
    }

    [Fact]
    public void BuildCell_AndEmitterConserveBlockCounts()
    {
        var target = new BuildCell(requiredBlocks: 2);
        var emitter = new BlockEmitter("emitter-01", initialInventory: 2);

        Assert.True(emitter.TryTakeBlock());
        Assert.True(target.TryPlaceBlock());
        Assert.True(emitter.TryTakeBlock());
        Assert.True(target.TryPlaceBlock());
        Assert.True(target.IsComplete);
        Assert.False(target.TryPlaceBlock());
        Assert.False(emitter.TryTakeBlock());
        Assert.Equal(0, emitter.Inventory);
        Assert.Equal(2, target.PlacedBlocks);
    }
}
